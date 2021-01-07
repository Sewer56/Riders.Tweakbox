﻿using System;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    // TODO: Add support for Spectator
    public unsafe class Host : Socket
    {
        /// <summary>
        /// Contains that is used by the server.
        /// </summary>
        public new HostState State => (HostState) base.State;

        public string Password { get; private set; }
        
        public Host(int port, string password, NetplayController controller) : base(controller)
        {
            Trace.WriteLine($"[Host] Hosting Server on {port} with password {password}");
            base.State = new HostState(IoC.GetConstant<NetplayImguiConfig>().ToHostPlayerData());
            Password   = password;
            Manager.Start(port);

            Event.OnSetSpawnLocationsStartOfRace += State.OnSetSpawnLocationsStartOfRace;
            Event.AfterSetSpawnLocationsStartOfRace += State.OnSetSpawnLocationsStartOfRace;

            Event.OnRace += OnRace;
            Event.AfterRace += AfterRace;
            Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHuman += State.OnCheckIfPlayerIsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator += State.OnCheckIfPlayerIsHuman;
            Initialize();
        }

        public override unsafe void Dispose()
        {
            Manager.DisconnectAll();
            base.Dispose();

            Event.OnSetSpawnLocationsStartOfRace -= State.OnSetSpawnLocationsStartOfRace;
            Event.AfterSetSpawnLocationsStartOfRace -= State.OnSetSpawnLocationsStartOfRace;

            Event.OnRace -= OnRace;
            Event.AfterRace -= AfterRace;
            Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHuman -= State.OnCheckIfPlayerIsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator -= State.OnCheckIfPlayerIsHuman;
        }

        public override SocketType GetSocketType() => SocketType.Host;

        public override void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.As<IPacket>().GetPacketType() == PacketKind.Reliable)
                HandleReliable(packet.Source, packet.As<ReliablePacket>());

            else if (packet.As<IPacket>().GetPacketType() == PacketKind.Unreliable)
                HandleUnreliable(packet.Source, packet.As<UnreliablePacket>());
        }

        private void HandleUnreliable(NetPeer peer, UnreliablePacket result)
        {
            // TODO: Maybe support for multiple local players in the future.
            var playerIndex = State.ClientMap.GetPlayerData(peer).PlayerIndex;
            var player  = result.Players[0];
            State.RaceSync[playerIndex] = player;
        }

        private void HandleReliable(NetPeer peer, ReliablePacket packet)
        {
            if (packet.SetMovementFlags.HasValue)
            {
                var playerIndex = State.ClientMap.GetPlayerData(peer).PlayerIndex;
                State.MovementFlagsSync[playerIndex] = new Timestamped<MovementFlagsMsg>(packet.SetMovementFlags.Value);
            }

            if (packet.Random.HasValue)
            {
                Trace.WriteLine("[Host] Received SRandSyncReady from Client.");
                State.ClientMap.GetCustomData(peer).SRandSyncReady = true;
            }
        }

        #region Events: On/After Events
        private void OnRace(Task<byte, RaceTaskState>* task)
        {
            Poll();
            State.ApplyRaceSync();
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            State.RaceSync[0] = new Timestamped<UnreliablePacketPlayer>(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
            
            // Populate data for non-expired packets.
            var players = new UnreliablePacketPlayer[State.GetPlayerCount()];
            Array.Fill(players, new UnreliablePacketPlayer());
            for (int x = 0; x < players.Length; x++)
            {
                var sync = State.RaceSync[x];
                if (!sync.IsDiscard(State.MaxLatency))
                    players[x] = sync;
                else
                    players[x] = UnreliablePacketPlayer.FromGame(x, State.FrameCounter);
            }

            // Broadcast data to all clients.
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var excludeIndex = State.ClientMap.GetPlayerData(peer).PlayerIndex;
                var packet = new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray());
                SendAndFlush(peer, packet, DeliveryMethod.Sequenced);
            }
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            State.OnAfterSetMovementFlags(player);

            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (index == 0)
            {
                State.MovementFlagsSync[0] = new MovementFlagsMsg(player);
                foreach (var peer in Manager.ConnectedPeerList)
                {
                    var excludeIndex  = State.ClientMap.GetPlayerData(peer).PlayerIndex;
                    var movementFlags = State.MovementFlagsSync.Where((timestamped, x) => x != excludeIndex).ToArray();
                    SendAndFlush(peer, new ReliablePacket() { MovementFlags = new MovementFlagsPacked().AsInterface().SetData(movementFlags, 0) } , DeliveryMethod.ReliableOrdered);
                }
            }

            return player;
        }
        #endregion

        #region Socket Events
        public override void OnPeerConnected(NetPeer peer)
        {
            bool CheckIfUserData(Packet<NetPeer> packet)
            {
                if (packet.As<IPacket>().GetPacketType() != PacketKind.Reliable)
                    return false;

                var reliable = packet.As<ReliablePacket>();
                if (!reliable.ServerMessage.HasValue)
                    return false;

                var message = reliable.ServerMessage.Value.Message;
                switch (message)
                {
                    case ClientSetPlayerData clientSetPlayerData:
                        State.ClientMap.AddOrUpdatePlayerData(peer, clientSetPlayerData.Data);
                        UpdatePlayerMap();
                        return true;
                }

                return false;
            }

            // Handle player handshake here!
            Trace.WriteLine($"[Host] Client {peer.EndPoint.Address} | {peer.Id}, waiting for message.");
            if (!TryWaitForMessage(peer, CheckIfUserData, State.HandshakeTimeout))
            {
                Trace.WriteLine($"[Host] Disconnecting client, did not receive user data.");
                peer.Disconnect();
                return;
            }
            
            SendAndFlush(peer, new ReliablePacket() { GameData = GameData.FromGame() }, DeliveryMethod.ReliableUnordered, "[Host] Received user data, uploading game data.");
            SendAndFlush(peer, new ReliablePacket(CourseSelectSync.FromGame(Event.CourseSelect)), DeliveryMethod.ReliableUnordered, "[Host] Sending course select data for initial sync.");
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            State.ClientMap.RemovePeer(peer);
            UpdatePlayerMap();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency.
            var data = State.ClientMap.GetCustomData(peer);
            if (data != null)
                data.Latency = latency;
        }

        public override bool OnConnectionRequest(ConnectionRequest request)
        {
            bool Reject(string message)
            {
                Trace.WriteLine(message);
                request.Reject();
                return false;
            }

            Trace.WriteLine($"[Host] Received Connection Request");
            if (Event.LastTask != Tasks.CourseSelect)
                return Reject("[Host] Rejected Connection | Not on Course Select");

            if (!State.ClientMap.HasEmptySlots())
                return Reject($"[Host] Rejected Connection | No Empty Slots");

            Trace.WriteLine($"[Host] Accepting if Password Matches");
            return request.AcceptIfKey(Password) != null;
        }
        #endregion

        #region Utility Functions
        /// <summary>
        /// Sends a personalized player map (excluding the player). 
        /// </summary>
        public void UpdatePlayerMap()
        {
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var message = State.ClientMap.ToMessage(peer, State.SelfInfo);
                SendAndFlush(peer, new ReliablePacket(message), DeliveryMethod.ReliableUnordered);
            }

            State.PlayerInfo = State.ClientMap.ToMessage(State.SelfInfo.PlayerIndex, State.SelfInfo).Data;
        }
        #endregion
    }
}