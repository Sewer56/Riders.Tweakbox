using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Misc;
using Constants = Riders.Netplay.Messages.Misc.Constants;

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
                try
                {
                    packet.Deserialize(rawBytes);
                    Socket.HandleUnreliablePacket(ref packet, peer);
                }
                catch (Exception e)
                {
                    packet.Dispose();
                    Log.WriteLine($"Exception Processing Unreliable Packet: {e.Message}");
                    throw;
                }
            }
            else
            {
                var packet = new ReliablePacket();
                try
                {
                    packet.Deserialize(rawBytes);
                    Socket.HandleReliablePacket(ref packet, peer);
                }
                catch (Exception e)
                {
                    packet.Dispose();
                    Log.WriteLine($"Exception Processing Reliable Packet: {e.Message}");
                    throw;
                }
            }
        }
    }
}
