using System;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public unsafe class Attack : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }

        /// <summary>
        /// True if there are any attacks to be executed.
        /// </summary>
        public bool HasAttacks => _attackSync.Any(x => !x.IsDiscard(Socket.State.MaxLatency) && x.Value.IsValid);

        /// <summary>
        /// This is set to true when the attacks received from host/client are being executed.
        /// This prevents messages from being relayed to host/client.
        /// </summary>
        private bool _isProcessingAttackPackets = false;

        /// <summary>
        /// Contains the synchronization data for handling attacks.
        /// </summary>
        private Timestamped<SetAttack>[] _attackSync = new Timestamped<SetAttack>[Constants.MaxNumberOfPlayers];

        public Attack(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;

            Event.OnShouldRejectAttackTask += OnShouldRejectAttackTask;
            Event.OnStartAttackTask += OnStartAttackTask;
            Event.AfterRace += AfterRace;
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
        public void HandlePacket(Packet<NetPeer> pkt)
        {
            if (pkt.GetPacketKind() != PacketKind.Reliable)
                return;
            
            var packet = pkt.As<ReliablePacket>();
            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (!packet.SetAttack.HasValue)
                    return;

                var hostState   = (HostState) Socket.State;
                var playerIndex = hostState.ClientMap.GetPlayerData(pkt.Source).PlayerIndex;
                Trace.WriteLine($"[{nameof(Attack)} / Host] Received Attack from {playerIndex} to hit {packet.SetAttack.Value.Target}");
                _attackSync[playerIndex] = new Timestamped<SetAttack>(packet.SetAttack.Value);
            }
            else
            {
                if (!packet.Attack.HasValue)
                    return;

                Trace.WriteLine($"[{nameof(Attack)} / Client] Received Attack data from host");
                var value   = packet.Attack.Value;
                var attacks = new SetAttack[_attackSync.Length];
                value.AsInterface().ToArray(attacks, attacks.Length - 1, 0, 1);
                for (var x = 0; x < attacks.Length; x++)
                {
                    attacks[x].Target = Socket.State.GetLocalPlayerIndex(attacks[x].Target);
                    _attackSync[x] = new Timestamped<SetAttack>(attacks[x]);
                }
            }
        }

        private unsafe int OnShouldRejectAttackTask(Player* playerOne, Player* playerTwo, int a3)
        {
            if (!_isProcessingAttackPackets)
            {
                var p1Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerOne);
                return p1Index != 0 ? 1 : 0;
            }

            return 0;
        }

        private unsafe int OnStartAttackTask(Player* playerOne, Player* playerTwo, int a3)
        {
            // Send attack notification to host if not 
            if (!_isProcessingAttackPackets)
            {
                var p1Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerOne);
                if (p1Index != 0)
                    return 0;

                var p2Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerTwo);
                switch (Socket.GetSocketType())
                {
                    case SocketType.Host:
                        Trace.WriteLine($"[{nameof(Attack)} / Host] Set Attack on {p2Index}");
                        _attackSync[0] = new Timestamped<SetAttack>(new SetAttack((byte)p2Index));
                        break;
                    case SocketType.Client:
                        Trace.WriteLine($"[{nameof(Attack)} / Client] Send Attack on {p2Index} [Host Index: {Socket.State.GetHostPlayerIndex(p2Index)}]");
                        Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { SetAttack = new SetAttack((byte)Socket.State.GetHostPlayerIndex(p2Index)) }, DeliveryMethod.ReliableOrdered);
                        break;

                    case SocketType.Spectator: break;
                }
            }

            return 0;
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (! HasAttacks)
                    goto exit;

                Trace.WriteLine($"[{nameof(Attack)} / Host] Sending Attack Matrix to Clients");
                foreach (var peer in Socket.Manager.ConnectedPeerList)
                {
                    var state = (HostState) Socket.State;
                    var excludeIndex = state.ClientMap.GetPlayerData(peer).PlayerIndex;
                    var attacks = _attackSync.Select(x => x.Value).Where((attack, x) => x != excludeIndex).ToArray();
                    if (!attacks.Any(x => x.IsValid))
                        continue;

                    #if DEBUG
                    for (var x = 0; x < attacks.Length; x++)
                    {
                        if (attacks[x].IsValid)
                            Trace.WriteLine($"[{nameof(Attack)} / Host] Send Attack Source ({x}), Target {attacks[x].Target}");
                    }
                    #endif

                    var packed = new AttackPacked().AsInterface().SetData(attacks);
                    Socket.SendAndFlush(peer, new ReliablePacket() { Attack = packed }, DeliveryMethod.ReliableOrdered);
                }
            }
            
            exit: 
            ProcessAttackTasks();
        }

        /// <summary>
        /// Processes all remaining attack tasks (from client/host) and resets them to the default value.
        /// </summary>
        public unsafe void ProcessAttackTasks()
        {
            _isProcessingAttackPackets = true;
            for (var x = 0; x < _attackSync.Length; x++)
            {
                if (x == 0)
                    continue;

                var atkSync = _attackSync[x];
                if (atkSync.IsDiscard(Socket.State.MaxLatency))
                    continue;

                var value = atkSync.Value;
                if (value.IsValid)
                {
                    Trace.WriteLine($"[State] Execute Attack by {x} on {value.Target}");
                    StartAttackTask(x, value.Target);
                }
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
    }
}
