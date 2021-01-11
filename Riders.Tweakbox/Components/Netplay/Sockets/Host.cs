using System.Diagnostics;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
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
            Manager.StartInManualMode(port);
            Initialize();
        }

        public override unsafe void Dispose()
        {
            Manager.DisconnectAll();
            base.Dispose();
        }

        public override SocketType GetSocketType() => SocketType.Host;
        public override void HandlePacket(Packet<NetPeer> packet) { }

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
            var data = State.ClientMap.GetPlayerData(peer);
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
