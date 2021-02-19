using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;

namespace Riders.Tweakbox.Components.Netplay.Components.Anticheat
{
    public class AntiIntegrity : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source)
        {
            throw new NotImplementedException();
        }
    }
}
