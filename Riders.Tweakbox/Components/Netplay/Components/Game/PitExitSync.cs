using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc.Log;
using StructLinq;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public unsafe class PitExitSync : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public CommonState State { get; set; }
    public EventController Event { get; set; }

    /// <summary>
    /// Contains the synchronization data for handling pits.
    /// </summary>
    private Timestamped<PitExit>[] _pitSync = new Timestamped<PitExit>[Constants.MaxNumberOfPlayers];
    private Logger _log = new Logger(LogCategory.Race);

    public PitExitSync(Socket socket, EventController @event)
    {
        Socket = socket;
        Event = @event;
        State = socket.State;

        EventController.SetForceExitPit += OnSetForceExitPit;
        EventController.OnExitPit += OnExitPit;
        Event.AfterRace += AfterRace;
    }

    public void Dispose()
    {
        EventController.SetForceExitPit -= OnSetForceExitPit;
        EventController.OnExitPit -= OnExitPit;
        Event.AfterRace -= AfterRace;
    }

    /// <summary>
    /// Resets the state of the current pit synchronization component.
    /// </summary>
    public void Reset()
    {
        Array.Fill(_pitSync, new Timestamped<PitExit>(new PitExit(false)));
    }

    /// <summary>
    /// Resets the state of the current pit synchronization component.
    /// </summary>
    public void ResetSelf()
    {
        Array.Fill(_pitSync, new Timestamped<PitExit>(new PitExit(false)), 0, State.SelfInfo.NumPlayers);
    }

    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {        
        // Get Attack Message
        if (packet.MessageType != MessageType.PitExit)
            return;

        _log.WriteLine($"[{nameof(PitExitSync)}] Received Pit Exit Packet");
        var pitExitPacked = packet.GetMessage<PitExitPacked>();

        // Index of first player to fill.
        int playerIndex = Socket.GetSocketType() switch
        {
            SocketType.Host   => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
            SocketType.Client => State.NumLocalPlayers,
            _ => throw new ArgumentOutOfRangeException()
        };

        var pits = pitExitPacked.Elements;
        var numPits = pitExitPacked.NumElements;
        for (int x = 0; x < numPits; x++)
        {
            var pit = pits[x];
            if (!pit.Exit)
                continue;

            _log.WriteLine($"[{nameof(PitExitSync)}] Received Pit Exit request from {playerIndex} [{x}]");
            _pitSync[playerIndex + x] = pit;
        }
    }

    private unsafe Enum<AsmFunctionResult> OnSetForceExitPit(Player* player)
    {        
        // Discard local players.
        var pitExiterIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        if (State.IsLocal(pitExiterIndex))
            return AsmFunctionResult.Indeterminate;

        // Force exit pit if needed.
        ref var exiter = ref _pitSync[pitExiterIndex];
        if (exiter.IsDiscard(Socket.State.MaxLatency))
            return AsmFunctionResult.Indeterminate;

        ref var value = ref exiter.Value;
        if (!value.Exit)
            return AsmFunctionResult.Indeterminate;

        // Reset value and force exit.
        value.Exit = false;
        _log.WriteLine($"[{nameof(PitExitSync)}] Forcing Pit Exit on {pitExiterIndex}");
        return AsmFunctionResult.True;
    }

    private void OnExitPit(Player* player)
    {
        // Discard non-local players.
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        if (!State.IsLocal(playerIndex))
            return;
        
        // Send pit exit notification to host.
        switch (Socket.GetSocketType())
        {
            case SocketType.Host:
                _log.WriteLine($"[{nameof(PitExitSync)} / Host] Set Pit Exit by {playerIndex}");
                _pitSync[playerIndex] = new Timestamped<PitExit>(new PitExit(true));
                break;

            case SocketType.Client when State.NumLocalPlayers > 0:
            {
                _log.WriteLine($"[{nameof(PitExitSync)} / Client] Send Pit Exit by {playerIndex}");

                // Make Packet
                using var attack = new PitExitPacked().CreatePooled(State.NumLocalPlayers);
                attack.Elements[playerIndex] = new PitExit(true);
                Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(attack), DeliveryMethod.ReliableOrdered);
                break;
            }
        }
    }

    private void AfterRace(Task<byte, RaceTaskState>* task)
    {
        // If host, broadcast pit information.
        if (Socket.GetSocketType() == SocketType.Host && HasAnyPits())
        {
            Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers]; // Player indices to exclude.

            for (var peerId = 0; peerId < Socket.Manager.ConnectedPeerList.Count; peerId++)
            {
                // Calculate some preliminary data.
                var peer = Socket.Manager.ConnectedPeerList[peerId];
                if (!((HostState)State).ClientMap.Contains(peer))
                    continue;

                var excludeIndices = Extensions.GetExcludeIndices((HostState)State, peer, excludeIndexBuffer);

                // Get all pit exits sans those made by players and their local friends;
                // then check if there are any pit exits they should be made aware of.
                using var pitExits = Extensions.GetItemsWithoutIndices(_pitSync.AsSpan(0, State.GetPlayerCount()), excludeIndices, State.MaxLatency);
                var pitExitArr = pitExits.Segment.Array;
                if (!pitExitArr.ToStructEnumerable().Any(x => x.Exit, x => x))
                    continue;

                // Transmit Packet Information
                using var pitExit = new PitExitPacked();
                pitExit.Set(pitExitArr, pitExits.Length);
                Socket.Send(peer, ReliablePacket.Create(pitExit), DeliveryMethod.ReliableOrdered);
            }

            _log.WriteLine($"[{nameof(PitExit)} / Host] Pit Exit Matrix Sent");
            Socket.Update();
            ResetSelf();
        }
    }

    /// <summary>
    /// Returns true if there are any players needing to exit pit.
    /// </summary>
    private bool HasAnyPits()
    {
        for (int x = 0; x < _pitSync.Length; x++)
        {
            var attack = _pitSync[x];
            if (!attack.IsDiscard(State.MaxLatency) && attack.Value.Exit)
                return true;
        }

        return false;
    }

    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}