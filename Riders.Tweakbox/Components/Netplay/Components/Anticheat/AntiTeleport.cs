﻿using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;

namespace Riders.Tweakbox.Components.Netplay.Components.Anticheat
{
    public class AntiTeleport : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}