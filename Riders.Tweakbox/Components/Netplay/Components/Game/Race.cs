using System;
using DotNext.Buffers;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Netplay.Messages.Unreliable;
using Riders.Netplay.Messages.Unreliable.Structs;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Constants = Riders.Netplay.Messages.Misc.Constants;
using Extensions = Riders.Tweakbox.Components.Netplay.Helpers.Extensions;
namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public unsafe class Race : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }

    public EventController Event { get; set; }
    public CommonState State { get; set; }
    public FramePacingController PacingController { get; set; }
    public NetplayEditorConfig.HostSettings HostSettings { get; set; }

    /// <summary>
    /// Jitter buffers for smoothing out incoming packets.
    /// There is a buffer per client.
    /// </summary>
    internal IJitterBuffer<UnreliablePacket>[] JitterBuffers = new IJitterBuffer<UnreliablePacket>[Constants.MaxNumberOfPlayers + 1];

    /// <summary>
    /// Sync data for races.
    /// </summary>
    private Volatile<UnreliablePacketPlayer>[] _raceSync = new Volatile<UnreliablePacketPlayer>[Constants.MaxNumberOfPlayers];

    /// <summary>
    /// Contains movement flags for each client.
    /// </summary>
    private Timestamped<Used<MovementFlags>>[] _movementFlags = new Timestamped<Used<MovementFlags>>[Constants.MaxNumberOfPlayers + 1];

    /// <summary>
    /// Contains inputs for each client.
    /// </summary>
    private Timestamped<AnalogXY>[] _analogXY = new Timestamped<AnalogXY>[Constants.MaxNumberOfPlayers + 1];

    private const DeliveryMethod RaceDeliveryMethod = DeliveryMethod.Unreliable;
    private readonly byte _raceChannel;
    private bool _isRacing;

    public Race(Socket socket, EventController @event)
    {
        Socket = socket;
        Event = @event;
        State = socket.State;
        HostSettings = Socket.Config.Data.HostSettings;
        PacingController = IoC.GetSingleton<FramePacingController>();

        _raceChannel = (byte)Socket.ChannelAllocator.GetChannel(RaceDeliveryMethod);
        Event.OnSetSpawnLocationsStartOfRace += SwapSpawns;
        Event.AfterSetSpawnLocationsStartOfRace += SwapSpawns;
        Event.AfterRunPhysicsSimulation += OnAfterPhysicsSimulation;

        Event.OnSetMovementFlagsOnInput += OnSetMovementFlagsOnInput;
        Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
        Event.OnCheckIfPlayerIsHumanInput += IsHuman;
        Event.OnCheckIfPlayerIsHumanIndicator += IsHuman;

        Event.OnRace += OnRace;
        Sewer56.SonicRiders.API.Event.OnKillAllTasks += OnKillAllTasks;

        var bufferSettings = Socket.Config.Data.PlayerSettings.BufferSettings;
        for (int x = 0; x < JitterBuffers.Length; x++)
            JitterBuffers[x] = IJitterBuffer<UnreliablePacket>.Create(bufferSettings.Type, bufferSettings.DefaultBufferSize, bufferSettings.NumJitterValuesSample, bufferSettings.MaxRampDownAmount);

        Reset();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Socket.ChannelAllocator.ReleaseChannel(RaceDeliveryMethod, _raceChannel);
        Event.OnSetSpawnLocationsStartOfRace -= SwapSpawns;
        Event.AfterSetSpawnLocationsStartOfRace -= SwapSpawns;
        Event.AfterRunPhysicsSimulation -= OnAfterPhysicsSimulation;

        Event.OnSetMovementFlagsOnInput -= OnSetMovementFlagsOnInput;
        Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
        Event.OnCheckIfPlayerIsHumanInput -= IsHuman;
        Event.OnCheckIfPlayerIsHumanIndicator -= IsHuman;

        Event.OnRace -= OnRace;
        Sewer56.SonicRiders.API.Event.OnKillAllTasks -= OnKillAllTasks;
    }

    public void Reset()
    {
        Array.Fill(_raceSync, new Volatile<UnreliablePacketPlayer>());
        Array.Fill(_movementFlags, new Timestamped<Used<MovementFlags>>());
        Array.Fill(_analogXY, new Timestamped<AnalogXY>());
        for (int x = 0; x < JitterBuffers.Length; x++)
            JitterBuffers[x].Clear();
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source)
    {
        // Index of first player to fill.
        int playerIndex = Socket.GetSocketType() switch
        {
            SocketType.Host => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
            SocketType.Client => State.NumLocalPlayers,
            _ => throw new ArgumentOutOfRangeException()
        };

        JitterBuffers[playerIndex].TryEnqueue(packet.Clone());
    }

    private void DequeueFromJitterBuffers()
    {
        for (int x = 0; x < JitterBuffers.Length; x++)
            DequeueFromJitterBuffer(x);
    }

    private void DequeueFromJitterBuffer(int playerIndex)
    {
        // Dequeue from buffer.
        var jitterBuffer = JitterBuffers[playerIndex];
        if (jitterBuffer.TryDequeue(playerIndex, out var packet))
            HandlePacket(playerIndex, packet);
    }

    private void HandlePacket(int playerIndex, UnreliablePacket packet)
    {
        try
        {
            var players = packet.Players;
            var numPlayers = packet.Header.NumberOfPlayers;

            for (int x = 0; x < numPlayers; x++)
            {
                _raceSync[playerIndex + x] = players[x];
                if (players[x].AnalogXY.HasValue)
                    _analogXY[playerIndex + x] = players[x].AnalogXY.Value;

                Extensions.ReplaceOrSetCurrentCachedItem(players[x].MovementFlags, _movementFlags, playerIndex + x, State.MaxLatency);
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine($"[{nameof(Race)}] Warning: Failed to Dequeue from Jitter Buffer | {ex.Message}",
                LogCategory.Race);
        }
        finally
        {
            packet.Dispose();
        }
    }

    private Player* OnSetMovementFlagsOnInput(Player* player)
    {
        // This is necessary so the game applies the up/down/left/right
        // flags accordingly which are necessary for drifting to function.
        ApplyAnalogStick(player);
        return player;
    }

    private Player* OnAfterSetMovementFlagsOnInput(Player* player)
    {
        ApplyMovementFlags(player);
        return player;
    }

    private int OnAfterPhysicsSimulation()
    {
        if (!_isRacing)
            return 0;

        Socket.Update();
        DequeueFromJitterBuffers();
        ApplyRaceSync();

        // Get Packet from the pool.
        using var packet = new UnreliablePacket(Constants.MaxNumberOfPlayers);

        // Update local player data.
        for (int x = 0; x < State.NumLocalPlayers; x++)
            _raceSync[x] = UnreliablePacketPlayer.FromGame(x);

        switch (Socket.GetSocketType())
        {
            case SocketType.Host:
            {
                // Populate data for non-expired packets.
                // TODO: 32-Player Support | Fix Called Function (UnreliablePacketPlayer.FromGame)
                using var players = new ArrayRental<UnreliablePacketPlayer>(State.GetPlayerCount());
                for (int x = 0; x < players.Length; x++)
                {
                    ref var sync = ref _raceSync[x];
                    if (!sync.HasValue)
                    {
                        players[x] = UnreliablePacketPlayer.FromGame(x);
                        continue;
                    }
                    
                    players[x] = sync.Get();
                }

                // Broadcast data to all clients.
                Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers]; // Player indices to exclude.

                for (var peerId = 0; peerId < Socket.Manager.ConnectedPeerList.Count; peerId++)
                {
                    var peer = Socket.Manager.ConnectedPeerList[peerId];
                    if (!((HostState)State).ClientMap.Contains(peer))
                        continue;

                    var excludeIndices = Extensions.GetExcludeIndices((HostState)State, peer, excludeIndexBuffer);
                    using var rental = Extensions.GetItemsWithoutIndices(players.Span, excludeIndices);

                    if (rental.Length <= 0)
                        continue;

                    // Construct packet.
                    packet.SetHeader(HostSettings.ReducedTickRate
                        ? new UnreliablePacketHeader((byte)rental.Length, State.FrameCounter, State.FrameCounter)
                        : new UnreliablePacketHeader((byte)rental.Length, State.FrameCounter));

                    for (int x = 0; x < rental.Length; x++)
                        packet.Players[x] = rental[x];

                    Socket.Send(peer, packet, RaceDeliveryMethod, _raceChannel);
                }

                break;
            }

            case SocketType.Client when State.NumLocalPlayers > 0:
                packet.SetHeader(new UnreliablePacketHeader((byte)State.NumLocalPlayers, State.FrameCounter));
                for (int x = 0; x < State.NumLocalPlayers; x++)
                    packet.Players[x] = UnreliablePacketPlayer.FromGame(x);

                Socket.Send(Socket.Manager.FirstPeer, packet, RaceDeliveryMethod, _raceChannel);
                break;
        }

        Socket.Update();
        return 0;
    }

    /// <summary>
    /// Applies the current race state obtained from clients/host to the game.
    /// </summary>
    private void ApplyRaceSync()
    {
        // Apply data of all players.
        for (int x = State.NumLocalPlayers; x < Constants.MaxRidersNumberOfPlayers; x++)
        {
            ref var sync = ref _raceSync[x];
            if (!sync.HasValue)
                continue;

            if (Socket.GetSocketType() == SocketType.Client)
                sync.Get().ToGame(x);
            else
                sync.GetNonvolatile().ToGame(x);
        }
    }

    private Enum<AsmFunctionResult> IsHuman(Player* player) => State.IsHuman(Sewer56.SonicRiders.API.Player.GetPlayerIndex(player));
    private void SwapSpawns(int numOfPlayers) => Sewer56.SonicRiders.API.Misc.SwapSpawnPositions(0, State.SelfInfo.PlayerIndex);

    /// <summary>
    /// Handles all movement flags to be applied to the client.
    /// </summary>
    private unsafe Player* ApplyMovementFlags(Player* player)
    {
        try
        {
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (State.IsLocal(index))
                return player;

            ref var flags = ref _movementFlags[index];
            if (!flags.IsDiscard(State.MaxLatency))
                flags.Value.UseValue().ToGame(player);
        }
        catch (Exception e)
        {
            Log.WriteLine($"[{nameof(Race)}] Failed to Apply Movement Flags {e.Message} {e.StackTrace}", LogCategory.Race);
        }

        return player;
    }

    /// <summary>
    /// Handles all analog stick inputs passed to the client.
    /// </summary>
    private unsafe Player* ApplyAnalogStick(Player* player)
    {
        try
        {
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (State.IsLocal(index))
                return player;

            ref var analogXY = ref _analogXY[index];
            if (!analogXY.IsDiscard(State.MaxLatency))
                analogXY.Value.ToGame(player);
        }
        catch (Exception e)
        {
            Log.WriteLine($"[{nameof(Race)}] Failed to Apply Movement Flags {e.Message} {e.StackTrace}", LogCategory.Race);
        }

        return player;
    }

    #region SetTask
    private void OnRace(Task<byte, RaceTaskState>* task) => _isRacing = true;
    private void OnKillAllTasks() => _isRacing = false;
    #endregion

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }
}
