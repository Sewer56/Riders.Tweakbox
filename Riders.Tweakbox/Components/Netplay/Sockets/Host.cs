using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public class Host : Socket
    {
        public string Password { get; private set; }
        public PlayerMap<PlayerState> PlayerMap { get; private set; } = new PlayerMap<PlayerState>();
        public int FrameCounter { get; private set; } = 0;

        public Host(int port, string password)
        {
            // TODO: Implement Connection
            Password = password;
            Manager.Start(port);
        }

        public override void Dispose()
        {
            Manager.DisconnectAll();
            base.Dispose();
        }

        public override bool IsHost() => true;
        public override void Update()
        {
            FrameCounter += 1;
            //UnreliablePacketPlayer.ShouldISend()
        }

        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {

        }

        public override void OnPeerConnected(NetPeer peer)
        {
            // Wait for user data acknowledgement.
            if (!TryWaitForMessage(peer, CheckIfUserData))
                peer.Disconnect();

            PlayerMap.AddPeer(peer);
            UpdatePlayerMap();

            bool CheckIfUserData(Packet packet) => packet.Reliable?.ServerMessage?.MessageKind == ServerMessageType.ClientSetPlayerData;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            UpdatePlayerMap();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency.
            var data = PlayerMap.GetCustomData(peer);
            if (data != null)
                data.Latency = latency;
        }

        public override void OnConnectionRequest(ConnectionRequest request)
        {
            // TODO: Check if in correct menu, and game mode matches.
            

            // After all other checks.
            request.AcceptIfKey(Password);
        }

        public void UpdatePlayerMap()
        {
            var message = PlayerMap.ToMessage();
            var serverMessage = new ServerMessage(message);
            var reliableMessage = new ReliablePacket() { ServerMessage = serverMessage };
            Manager.SendToAll(reliableMessage.Serialize(), DeliveryMethod.ReliableSequenced);
        }
    }
}
