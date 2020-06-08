using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public class Client : Socket
    {
        public Client(string ipAddress, int port, string password)
        {
            // TODO: Implement Connection
            Manager.Start();
            Manager.Connect(ipAddress, port, password);
        }

        
        public override bool IsHost() => false;
        public override void Update()
        {

        }

        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {

        }

        public override void OnPeerConnected(NetPeer peer)
        {
            // Inform host of player data.
            var playerData = IoC.GetConstant<NetplayImguiConfig>();
            var setPlayerData = new ClientSetPlayerData() { Data = playerData.FromImguiData() };
            var message = new ServerMessage(setPlayerData);
            var packet = new ReliablePacket { ServerMessage = message };
            peer.Send(packet.Serialize(), DeliveryMethod.ReliableUnordered);
            peer.Flush();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override void OnConnectionRequest(ConnectionRequest request) { }
    }
}
