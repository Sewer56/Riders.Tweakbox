using System;
using System.Linq;
using LiteNetLib;
using Reloaded.Memory;
using Reloaded.Memory.Sources;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public unsafe class Race : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public CommonState State { get; set; }

        /// <summary>
        /// Sync data for races.
        /// </summary>
        private Volatile<Timestamped<UnreliablePacketPlayer>>[] _raceSync = new Volatile<Timestamped<UnreliablePacketPlayer>>[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Contains movement flags for each client.
        /// </summary>
        private Timestamped<Used<MovementFlagsMsg>>[] _movementFlags = new Timestamped<Used<MovementFlagsMsg>>[Constants.MaxNumberOfPlayers + 1];

        private DeliveryMethod _movementFlagsDeliveryMethod = DeliveryMethod.ReliableOrdered;
        private DeliveryMethod _raceDeliveryMethod = DeliveryMethod.Sequenced;
        private readonly byte _raceChannel;
        
        public Race(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;
            State = socket.State;

            _raceChannel = (byte)Socket.ChannelAllocator.GetChannel(_raceDeliveryMethod);
            Event.OnSetSpawnLocationsStartOfRace += SwapSpawns;
            Event.AfterSetSpawnLocationsStartOfRace += SwapSpawns;

            Event.OnRace += OnRace;
            Event.AfterRace += AfterRace;
            Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHumanInput += IsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator += IsHuman;

            Reset();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Socket.ChannelAllocator.ReleaseChannel(_raceDeliveryMethod, _raceChannel);
            Event.OnSetSpawnLocationsStartOfRace -= SwapSpawns;
            Event.AfterSetSpawnLocationsStartOfRace -= SwapSpawns;

            Event.OnRace -= OnRace;
            Event.AfterRace -= AfterRace;
            Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHumanInput -= IsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator -= IsHuman;
        }


        public void Reset()
        {
            Array.Fill(_raceSync, new Volatile<Timestamped<UnreliablePacketPlayer>>());
            Array.Fill(_movementFlags, new Timestamped<Used<MovementFlagsMsg>>());
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.GetPacketKind() == PacketKind.Unreliable)
                HandleUnreliablePacket(packet.Source, packet.As<UnreliablePacket>());

            else if (packet.GetPacketKind() == PacketKind.Reliable)
                HandleReliablePacket(packet.Source, packet.As<ReliablePacket>());
        }

        private void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (packet.SetMovementFlags.HasValue)
                {
                    var hostState = (HostState) State;
                    var playerIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;

                    // Replace if used, or merge.
                    ReplaceOrSetCurrentFlags(packet.SetMovementFlags.Value, playerIndex);
                }
            }
            else if (Socket.GetSocketType() == SocketType.Client)
            {
                // TODO: Spectator Support
                if (packet.MovementFlags.HasValue)
                {
                    var packedFlags = packet.MovementFlags.Value.AsInterface();
                    for (int x = 0; x < packedFlags.NumElements; x++)
                    {
                        // Fill in from player 2.
                        ReplaceOrSetCurrentFlags(packedFlags.Elements[x], x + 1);
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"Not Implemented");
            }

            // Local Function
            void ReplaceOrSetCurrentFlags(MovementFlagsMsg movementFlags, int playerIndex)
            {
                ref var currentFlags = ref _movementFlags[playerIndex];
                if (currentFlags.IsDiscard(State.MaxLatency) || currentFlags.Value.IsUsed)
                    currentFlags = new Timestamped<Used<MovementFlagsMsg>>(movementFlags);
                else
                    currentFlags.Value.Value.Merge(movementFlags);
            }
        }

        private void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                // TODO: Maybe support for multiple local players in the future.
                try
                {
                    var hostState = (HostState)State;
                    var playerIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                    var player = packet.Players[0];
                    _raceSync[playerIndex] = new Volatile<Timestamped<UnreliablePacketPlayer>>(player);
                }
                catch (Exception e)
                {
                    Log.WriteLine($"[{nameof(Race)}] Warning: Failed to update Race Sync", LogCategory.Race);
                }
            }
            else
            {
                var players = packet.Players;

                // Fill in from player 2.
                for (int x = 0; x < players.Length; x++)
                    _raceSync[x + 1] = new Volatile<Timestamped<UnreliablePacketPlayer>>(players[x]);
            }
        }

        private void OnRace(Task<byte, RaceTaskState>* task)
        {
            Socket.Update();
            ApplyRaceSync();
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                _raceSync[0] = new Timestamped<UnreliablePacketPlayer>(UnreliablePacketPlayer.FromGame(0));

                // Populate data for non-expired packets.
                var players = new UnreliablePacketPlayer[State.GetPlayerCount()];
                Array.Fill(players, new UnreliablePacketPlayer());
                for (int x = 0; x < players.Length; x++)
                {
                    var sync = _raceSync[x];
                    if (!sync.HasValue)
                    {
                        players[x] = UnreliablePacketPlayer.FromGame(x);
                        continue;
                    }

                    var syncStamped = sync.GetNonvolatile();
                    if (!syncStamped.IsDiscard(State.MaxLatency))
                        players[x] = syncStamped;
                    else
                        players[x] = UnreliablePacketPlayer.FromGame(x);
                }

                // Broadcast data to all clients.
                var hostState = (HostState)State;
                for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
                {
                    var peer = Socket.Manager.ConnectedPeerList[x];
                    var excludeIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                    var packet = Socket.Config.Data.ReducedTickRate 
                        ? new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray(), State.FrameCounter) 
                        : new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray());

                    Socket.Send(peer, packet, _raceDeliveryMethod, _raceChannel);
                }

                Socket.Update();
            }
            else
            {
                var packet = new UnreliablePacket(UnreliablePacketPlayer.FromGame(0));
                Socket.SendAndFlush(Socket.Manager.FirstPeer, packet, _raceDeliveryMethod, _raceChannel);
            }
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            ApplyMovementFlags(player);

            if (Socket.GetSocketType() == SocketType.Host)
            {
                var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
                if (index == 0)
                {
                    var hostState = (HostState) State;
                    _movementFlags[0] = new Timestamped<Used<MovementFlagsMsg>>(new MovementFlagsMsg(player));
                    for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
                    {
                        var peer          = Socket.Manager.ConnectedPeerList[x];
                        var excludeIndex  = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                        var movementFlags = _movementFlags.Where((timestamped, x) => x != excludeIndex).Select(x => x.Value.Value).ToArray();
                        Socket.Send(peer, new ReliablePacket() { MovementFlags = new MovementFlagsPacked().AsInterface().Create(movementFlags) }, _movementFlagsDeliveryMethod);
                    }
                }

                return player;
            }
            else
            {
                var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
                if (index == 0)
                    Socket.Send(Socket.Manager.FirstPeer, new ReliablePacket() { SetMovementFlags = new MovementFlagsMsg(player) }, _movementFlagsDeliveryMethod);

                return player;
            }
        }

        /// <summary>
        /// Applies the current race state obtained from clients/host to the game.
        /// </summary>
        private void ApplyRaceSync()
        {
            // Apply data of all players.
            // TODO: Update for spectator.
            for (int x = 1; x < _raceSync.Length; x++)
            {
                var sync = _raceSync[x];
                if (!sync.HasValue) 
                    continue;

                var syncStamped = Socket.GetSocketType() == SocketType.Host ? sync.GetNonvolatile() : sync.Get();
                if (syncStamped.IsDiscard(State.MaxLatency))
                    continue;

                if (syncStamped.Value.IsDefault())
                {
                    Log.WriteLine("Discarding Race Packet due to Default Comparison", LogCategory.Race);
                    continue;
                }

                syncStamped.Value.ToGame(x);
            }
        }

        private Enum<AsmFunctionResult> IsHuman(Player* player) => State.IsHuman(Sewer56.SonicRiders.API.Player.GetPlayerIndex(player));
        private void SwapSpawns(int numOfPlayers) => Sewer56.SonicRiders.API.Misc.SwapSpawnPositions(0, State.SelfInfo.PlayerIndex);

        /// <summary>
        /// Handles all Boost/Tornado/Attack tasks received from the clients.
        /// </summary>
        private unsafe Player* ApplyMovementFlags(Player* player)
        {
            try
            {
                var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

                // TODO: Handle Spectator
                if (index == 0)
                    return player;

                ref var flags = ref _movementFlags[index];
                if (flags.IsDiscard(State.MaxLatency))
                    return player;

                var flagData = flags.Value.UseValue();
                flagData.ToGame(player);
            }
            catch (Exception e)
            {
                Log.WriteLine($"[{nameof(Race)}] Failed to Apply Movement Flags {e.Message} {e.StackTrace}", LogCategory.Race);
            }

            return player;
        }
    }
}
