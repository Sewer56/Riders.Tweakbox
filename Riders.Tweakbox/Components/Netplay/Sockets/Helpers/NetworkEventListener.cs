using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class NetworkEventListener : EventBasedNetListener, IDisposable
    {
        public Socket Socket;

        public NetworkEventListener(Socket socket)
        {
            Socket = socket;
            base.NetworkReceiveEvent += NetworkReceive;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            base.NetworkReceiveEvent -= NetworkReceive;
        }

        public void NetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var rawBytes = reader.RawData.AsSpan(reader.UserDataOffset, reader.UserDataSize);
            if (deliveryMethod == DeliveryMethod.Sequenced || deliveryMethod == DeliveryMethod.Unreliable)
            {
                var packet = new UnreliablePacket(Constants.MaxNumberOfPlayers);
                packet.Deserialize(rawBytes);
                Socket.HandleUnreliablePacket(ref packet, peer);
                packet.Dispose();
            }
            else
            {
                var packet = new ReliablePacket();
                packet.Deserialize(rawBytes);
                Socket.HandleReliablePacket(ref packet, peer);
                packet.Dispose();
            }
        }
    }
}
