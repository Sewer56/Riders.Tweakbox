using System;
using LiteNetLib;
using Riders.Netplay.Messages;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class EventListener : EventBasedNetListener
    {
        public Socket Socket;
        public event HandleReliablePacket   OnHandleReliablePacket;
        public event HandleUnreliablePacket OnHandleUnreliablePacket;
        public event HandlePacket<NetPeer>  OnHandlePacket;

        public EventListener(Socket socket)
        {
            Socket = socket;
            base.PeerConnectedEvent += PeerConnected;
            base.PeerDisconnectedEvent += PeerDisconnected;
            base.NetworkReceiveEvent += NetworkReceive;
            base.NetworkLatencyUpdateEvent += NetworkLatencyUpdate;
            base.ConnectionRequestEvent += ConnectionRequest;
            OnHandleReliablePacket += Socket.HandleReliablePacket;
            OnHandleUnreliablePacket += Socket.HandleUnreliablePacket;
        }

        public void PeerConnected(NetPeer peer) => Socket.OnPeerConnected(peer);
        public void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Socket.OnPeerDisconnected(peer, disconnectInfo);
        public void NetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var rawBytes = reader.GetRemainingBytes().AsSpan();
            if (deliveryMethod == DeliveryMethod.Sequenced || deliveryMethod == DeliveryMethod.Unreliable)
            {
                var packet = UnreliablePacket.Deserialize(rawBytes);
                OnHandleUnreliablePacket?.Invoke(peer, packet);
                OnHandlePacket?.Invoke(new Packet<NetPeer>(peer, null, packet));
            }
            else
            {
                var packet = ReliablePacket.Deserialize(rawBytes);
                OnHandleReliablePacket?.Invoke(peer, packet);
                OnHandlePacket?.Invoke(new Packet<NetPeer>(peer, packet, null));
            }
        }

        public void NetworkLatencyUpdate(NetPeer peer, int latency) => Socket.OnNetworkLatencyUpdate(peer, latency);
        public void ConnectionRequest(ConnectionRequest request) => Socket.OnConnectionRequest(request);

        #region Delegates
        public delegate void HandlePacket<T>(Packet<T> packet);
        public delegate void HandleReliablePacket(NetPeer peer, ReliablePacket packet);
        public delegate void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet);
        #endregion
    }
}
