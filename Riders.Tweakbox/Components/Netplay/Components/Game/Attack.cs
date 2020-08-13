using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class Attack : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.GetPacketKind() != PacketKind.Reliable)
                return;

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
