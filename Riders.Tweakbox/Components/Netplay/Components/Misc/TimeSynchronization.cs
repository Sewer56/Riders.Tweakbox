using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    /// <summary>
    /// Synchronizes time to a network server using the NTP protocol at a fixed interval.
    /// We use this to synchronize events in real time like race start.
    /// </summary>
    public class TimeSynchronization : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public NetManager Manager { get; set; }

        public TimeSynchronization(Socket socket)
        {
            Socket = socket;
            Manager = socket.Manager;
        }

        /// <inheritdoc />
        public void Dispose() { }

        /// <summary>
        /// [For Client]
        /// Converts local time to server time.
        /// </summary>
        public DateTime ToServerTime(DateTime time)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                return time;
            
            return time + TimeSpan.FromTicks(Manager.FirstPeer.RemoteTimeDelta);
        }

        /// <summary>
        /// [For Client]
        /// Converts server time to local time.
        /// </summary>
        public DateTime ToLocalTime(DateTime time)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                return time;

            return time - TimeSpan.FromTicks(Manager.FirstPeer.RemoteTimeDelta);
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
