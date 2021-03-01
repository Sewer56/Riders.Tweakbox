using System;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using StructLinq;

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

            return time + TimeSpan.FromTicks(GetAccurateRemoteTimeDelta());
        }

        /// <summary>
        /// [For Client]
        /// Converts server time to local time.
        /// </summary>
        public DateTime ToLocalTime(DateTime time)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                return time;

            return time - TimeSpan.FromTicks(GetAccurateRemoteTimeDelta());
        }

        private long GetAccurateRemoteTimeDelta()
        {
            // Remote time delta without library calculated ping.
            var recentLatencies             = Socket.State.GetHostData().RecentLatencies;
            var remoteTimeDeltaWithoutPing  = Manager.FirstPeer.RemoteTimeDelta - ((Socket.State.GetHostData().Latency + 0.5) * TimeSpan.TicksPerMillisecond);
            var averagePing                 = recentLatencies.ToStructEnumerable().Sum(x => x.Value + 0.5, x => x) / recentLatencies.Count;
            return (long) (remoteTimeDeltaWithoutPing + (averagePing * TimeSpan.TicksPerMillisecond));
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
