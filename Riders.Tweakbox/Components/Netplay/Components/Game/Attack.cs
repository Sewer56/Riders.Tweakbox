using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using StructLinq;
using Constants = Riders.Netplay.Messages.Misc.Constants;
using Extensions = Riders.Tweakbox.Components.Netplay.Helpers.Extensions;
using Functions = Sewer56.SonicRiders.Functions.Functions;
namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public unsafe class Attack : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public CommonState State { get; set; }
    public EventController Event { get; set; }

    /// <summary>
    /// True if there are any attacks to be executed.
    /// </summary>
    public bool HasAttacks => HasAnyAttacks();

    /// <summary>
    /// This is set to true when the attacks received from host/client are being executed.
    /// This prevents messages from being relayed to host/client.
    /// </summary>
    private bool _isProcessingAttackPackets = false;

    /// <summary>
    /// Contains the synchronization data for handling attacks.
    /// </summary>
    private Timestamped<SetAttack>[] _attackSync = new Timestamped<SetAttack>[Constants.MaxNumberOfPlayers];
    private GameModifiers _modifiers;
    private Logger _log = new Logger(LogCategory.Race);

    public Attack(Socket socket, EventController @event)
    {
        Socket = socket;
        Event = @event;
        State = Socket.State;
        Socket.TryGetComponent(out _modifiers);

        Event.OnShouldRejectAttackTask += OnShouldRejectAttackTask;
        Event.OnStartAttackTask += OnStartAttackTask;
        Event.AfterRace += AfterRace;
        Reset();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Event.OnShouldRejectAttackTask -= OnShouldRejectAttackTask;
        Event.OnStartAttackTask -= OnStartAttackTask;
        Event.AfterRace -= AfterRace;
    }

    /// <summary>
    /// Resets the state of the current attack synchronization component.
    /// </summary>
    public void Reset()
    {
        Array.Fill(_attackSync, new Timestamped<SetAttack>(new SetAttack(false, 0)));
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        // Get Attack Message
        if (packet.MessageType != MessageType.Attack)
            return;

        _log.WriteLine($"[{nameof(Attack)}] Received Attack Packet");
        var attackPacked = packet.GetMessage<AttackPacked>();

        // Index of first player to fill.
        int playerIndex = Socket.GetSocketType() switch
        {
            SocketType.Host => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
            SocketType.Client => State.NumLocalPlayers,
            _ => throw new ArgumentOutOfRangeException()
        };

        var attacks = attackPacked.Elements;
        var numAttacks = attackPacked.NumElements;
        for (int x = 0; x < numAttacks; x++)
        {
            var attack = attacks[x];
            if (!attack.IsValid)
                continue;

            attack.Target = (byte)State.GetLocalPlayerIndex(attack.Target);

            _log.WriteLine($"[{nameof(Attack)}] Received Attack from {playerIndex} [{x}] to hit {attack.Target}");
            _attackSync[playerIndex + x] = attack;
        }
    }

    private unsafe int OnShouldRejectAttackTask(Player* playerOne, Player* playerTwo, int a3)
    {
        // Discard attacks from other players.
        if (_isProcessingAttackPackets)
            return 0;

        if (_modifiers != null && _modifiers.Modifiers.DisableAttacks)
            return 1;

        var p1Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerOne);
        return !State.IsLocal(p1Index) ? 1 : 0;
    }

    private unsafe int OnStartAttackTask(Player* playerOne, Player* playerTwo, int a3)
    {
        // Send attack notification to host if not processing external attacks.
        if (_isProcessingAttackPackets)
            return 0;

        // Discard non-local players.
        var attackerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerOne);
        if (!State.IsLocal(attackerIndex))
            return 0;

        var attackedIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerTwo);
        switch (Socket.GetSocketType())
        {
            case SocketType.Host:
                _log.WriteLine($"[{nameof(Attack)} / Host] Set Attack by {attackerIndex} on {attackedIndex}");
                _attackSync[attackerIndex] = new Timestamped<SetAttack>(new SetAttack((byte)attackedIndex));
                break;

            case SocketType.Client when State.NumLocalPlayers > 0:
                {
                    var hostIndex = Socket.State.GetHostPlayerIndex(attackedIndex);
                    _log.WriteLine($"[{nameof(Attack)} / Client] Send Attack by {attackerIndex} on {attackedIndex} [Host Index: {hostIndex}]");

                    // Make Packet
                    using var attack = new AttackPacked().CreatePooled(State.NumLocalPlayers);
                    attack.Elements[attackerIndex] = new SetAttack((byte)hostIndex);
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(attack), DeliveryMethod.ReliableOrdered);
                    break;
                }
        }

        return 0;
    }

    private void AfterRace(Task<byte, RaceTaskState>* task)
    {
        if (Socket.GetSocketType() == SocketType.Host && HasAttacks)
        {
            Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers]; // Player indices to exclude.

            for (var peerId = 0; peerId < Socket.Manager.ConnectedPeerList.Count; peerId++)
            {
                // Calculate some preliminary data.
                var peer = Socket.Manager.ConnectedPeerList[peerId];
                if (!((HostState)State).ClientMap.Contains(peer))
                    continue;

                var excludeIndices = Extensions.GetExcludeIndices((HostState)State, peer, excludeIndexBuffer);

                // Get all attacks sans those made by players and their local players;
                // then check if there are any attacks they should be made aware of.
                using var attacks = Extensions.GetItemsWithoutIndices(_attackSync.AsSpan(0, State.GetPlayerCount()), excludeIndices, State.MaxLatency);
                var attacksArr = attacks.Segment.Array;
                if (!attacksArr.ToStructEnumerable().Any(x => x.IsValid, x => x))
                    continue;

#if DEBUG
                for (var y = 0; y < attacks.Length; y++)
                {
                    if (attacks[y].IsValid)
                        _log.WriteLine($"[{nameof(Attack)} / Host] Send Attack Source ({y}), Target {attacks[y].Target}");
                }
#endif

                // Transmit Packet Information
                using var attack = new AttackPacked();
                attack.Set(attacksArr, attacks.Length);
                Socket.Send(peer, ReliablePacket.Create(attack), DeliveryMethod.ReliableOrdered);
            }

            _log.WriteLine($"[{nameof(Attack)} / Host] Attack Matrix Sent");
            Socket.Update();
        }

        ProcessAttackTasks();
    }

    /// <summary>
    /// Processes all remaining attack tasks (from client/host) and resets them to the default value.
    /// </summary>
    public unsafe void ProcessAttackTasks()
    {
        _isProcessingAttackPackets = true;

        for (var x = State.NumLocalPlayers; x < Constants.MaxRidersNumberOfPlayers; x++)
        {
            var atkSync = _attackSync[x];
            if (atkSync.IsDiscard(Socket.State.MaxLatency))
                continue;

            var value = atkSync.Value;
            if (!value.IsValid)
                continue;

            _log.WriteLine($"[State] Execute Attack by {x} on {value.Target}");
            StartAttackTask(x, value.Target);
        }

        Reset();
        _isProcessingAttackPackets = false;
    }

    /// <summary>
    /// Starts an attack between two players.
    /// </summary>
    /// <param name="playerOne">The attacking player index.</param>
    /// <param name="playerTwo">The player to be attacked index.</param>
    /// <param name="a3">Unknown Parameter</param>
    public unsafe void StartAttackTask(int playerOne, int playerTwo, int a3 = 1)
    {
        Functions.StartAttackTask.GetWrapper()(&Sewer56.SonicRiders.API.Player.Players.Pointer[playerOne], &Sewer56.SonicRiders.API.Player.Players.Pointer[playerTwo], a3);
    }

    /// <summary>
    /// True if there are any attacks to be transmitted, else false.
    /// </summary>
    private bool HasAnyAttacks()
    {
        for (int x = 0; x < _attackSync.Length; x++)
        {
            var attack = _attackSync[x];
            if (!attack.IsDiscard(State.MaxLatency) && attack.Value.IsValid)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
