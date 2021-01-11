using System;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

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
        private MovementFlagsMsg[] _movementFlags = new MovementFlagsMsg[Constants.MaxNumberOfPlayers];

        public Race(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;
            State = socket.State;

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
            Array.Fill(_movementFlags, new Timestamped<MovementFlagsMsg>());
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
                    _movementFlags[playerIndex] = new Timestamped<MovementFlagsMsg>(packet.SetMovementFlags.Value);
                }
            }
            else
            {
                // TODO: Spectator Support
                if (packet.MovementFlags.HasValue)
                {
                    packet.MovementFlags.Value.AsInterface().ToArray(_movementFlags, MovementFlagsPacked.NumberOfEntries - 1, 0, 1);
                }
            }
        }

        private void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                // TODO: Maybe support for multiple local players in the future.
                var hostState   = (HostState) State;
                var playerIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                var player = packet.Players[0];
                _raceSync[playerIndex] = new Volatile<Timestamped<UnreliablePacketPlayer>>(player);
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
                _raceSync[0] = new Timestamped<UnreliablePacketPlayer>(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));

                // Populate data for non-expired packets.
                var players = new UnreliablePacketPlayer[State.GetPlayerCount()];
                Array.Fill(players, new UnreliablePacketPlayer());
                for (int x = 0; x < players.Length; x++)
                {
                    var sync = _raceSync[x];
                    if (!sync.HasValue)
                        continue;

                    var syncStamped = sync.Get();
                    if (!syncStamped.IsDiscard(State.MaxLatency))
                        players[x] = syncStamped;
                    else
                        players[x] = UnreliablePacketPlayer.FromGame(x, State.FrameCounter);
                }

                // Broadcast data to all clients.
                var hostState = (HostState)State;
                foreach (var peer in Socket.Manager.ConnectedPeerList)
                {
                    var excludeIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                    var packet = new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray());
                    Socket.SendAndFlush(peer, packet, DeliveryMethod.Sequenced);
                }
            }
            else
            {
                var packet = new UnreliablePacket(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
                Socket.SendAndFlush(Socket.Manager.FirstPeer, packet, DeliveryMethod.Sequenced);
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
                    _movementFlags[0] = new MovementFlagsMsg(player);
                    foreach (var peer in Socket.Manager.ConnectedPeerList)
                    {
                        var excludeIndex = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                        var movementFlags = _movementFlags.Where((timestamped, x) => x != excludeIndex).ToArray();
                        Socket.SendAndFlush(peer, new ReliablePacket() { MovementFlags = new MovementFlagsPacked().AsInterface().SetData(movementFlags, 0) }, DeliveryMethod.ReliableOrdered);
                    }
                }

                return player;
            }
            else
            {
                var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
                if (index == 0)
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { SetMovementFlags = new MovementFlagsMsg(player) }, DeliveryMethod.ReliableOrdered);

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

                var syncStamped = sync.Get();
                if (syncStamped.IsDiscard(State.MaxLatency))
                    continue;

                if (syncStamped.Value.IsDefault())
                {
                    Trace.WriteLine("Discarding Race Packet due to Default Comparison");
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
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            // TODO: Handle Spectator
            if (index == 0)
                return player;

            _movementFlags[index].ToGame(player);
            return player;
        }
    }
}
