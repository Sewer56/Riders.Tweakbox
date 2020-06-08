using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Riders.Netplay.Messages;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Components
{
    public class EventListener : INetEventListener
    {
        public bool IsClient;
        public ISocket Socket;
        
        public EventListener(ISocket socket, bool isClient)
        {
            Socket   = socket;
            IsClient = isClient;
        }

        public void OnPeerConnected(NetPeer peer)
        {

        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {

        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var rawBytes = reader.GetRemainingBytes().AsSpan();
            if (deliveryMethod == DeliveryMethod.Sequenced || deliveryMethod == DeliveryMethod.Unreliable)
                Socket.HandleUnreliablePacket(UnreliablePacket.Deserialize(rawBytes));
            else
                Socket.HandleReliablePacket(ReliablePacket.Deserialize(rawBytes));
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {

        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
    }
}
