using System;
using Riders.Netplay.Messages;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public interface ISocket : IDisposable
    {
        /// <summary>
        /// True is connected, else false.
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// True if the socket is a host, else false.
        /// </summary>
        bool IsHost();

        /// <summary>
        /// Updates the current socket state.
        /// </summary>
        void Update();

        /// <summary>
        /// Handles an individual reliable packet of data.
        /// </summary>
        void HandleReliablePacket(ReliablePacket packet);

        /// <summary>
        /// Handles an individual unreliable packet of data.
        /// </summary>
        void HandleUnreliablePacket(UnreliablePacket packet);
    }
}