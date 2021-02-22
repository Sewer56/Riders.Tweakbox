using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;

namespace Riders.Tweakbox.Components.Netplay.Components
{
    /// <summary>
    /// Represents a component for an individual part of Netplay.
    /// </summary>
    public interface INetplayComponent : IDisposable
    {
        /// <summary>
        /// Gets the socket this component belongs to.
        /// </summary>
        Socket Socket { get; set; }

        /// <summary>
        /// Handles a packet with a given type parameters.
        /// </summary>
        void HandleReliablePacket(ref ReliablePacket packet, NetPeer source);

        /// <summary>
        /// Handles a packet with a given type parameters.
        /// </summary>
        void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source);
    }
}