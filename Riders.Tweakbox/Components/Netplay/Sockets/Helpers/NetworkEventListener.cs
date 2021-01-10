using System;
using LiteNetLib;
using Riders.Netplay.Messages;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class NetworkEventListener : EventBasedNetListener
    {
        public Socket Socket;
        public event HandlePacketFn<NetPeer>  OnQueuePacket;

        public NetworkEventListener(Socket socket)
        {
            Socket = socket;
            base.PeerConnectedEvent += PeerConnected;
            base.PeerDisconnectedEvent += PeerDisconnected;
            base.NetworkReceiveEvent += NetworkReceive;
            base.NetworkLatencyUpdateEvent += NetworkLatencyUpdate;
            base.ConnectionRequestEvent += ConnectionRequest;
            OnQueuePacket += QueuePacket;
        }

        public void QueuePacket(Packet<NetPeer> packet) => Socket.Queue.Enqueue(packet);
        public void PeerConnected(NetPeer peer) => Socket.OnPeerConnected(peer);
        public void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Socket.OnPeerDisconnected(peer, disconnectInfo);
        public void NetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var rawBytes = reader.RawData.AsSpan(reader.Position);
            if (deliveryMethod == DeliveryMethod.Sequenced || deliveryMethod == DeliveryMethod.Unreliable)
            {
                var packet = IPacket<UnreliablePacket>.FromSpan(rawBytes);
                OnQueuePacket?.Invoke(new Packet<NetPeer>(peer, packet));
            }
            else
            {
                var packet = IPacket<ReliablePacket>.FromSpan(rawBytes);
                OnQueuePacket?.Invoke(new Packet<NetPeer>(peer, packet));
            }
        }

        public void NetworkLatencyUpdate(NetPeer peer, int latency) => Socket.OnNetworkLatencyUpdate(peer, latency);
        public void ConnectionRequest(ConnectionRequest request) => Socket.OnConnectionRequest(request);

        #region Delegates
        public delegate void HandlePacketFn<T>(Packet<T> packet);
        #endregion
    }
}
