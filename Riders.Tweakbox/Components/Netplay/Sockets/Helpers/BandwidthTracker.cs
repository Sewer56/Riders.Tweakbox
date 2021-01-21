using System.Threading;
using LiteNetLib;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Tracks bandwidth over course of time for a given network manager.
    /// </summary>
    public class BandwidthTracker
    {
        public const int UdpPacketSize = 20 + 8;

        public int BytesSent     { get; private set; }
        public int BytesReceived { get; private set; }
        public int PacketsSent   { get; private set; }
        public int PacketsReceived { get; private set; }

        // Estimate of packet overhead
        public float KBytesPacketOverheadSent     => (PacketsSent * UdpPacketSize) / 1000.0f;
        public float KBytesPacketOverheadReceived => (PacketsReceived * UdpPacketSize) / 1000.0f;

        // Amount of raw data sent.
        public float KBytesSent =>     BytesSent / 1000.0f;
        public float KBytesReceived => BytesReceived / 1000.0f;

        // Raw data sent + overhead.
        public float KBytesSentWithOverhead     => KBytesSent + KBytesPacketOverheadSent;
        public float KBytesReceivedWithOverhead => KBytesReceived + KBytesPacketOverheadReceived;

        public NetManager Manager;
        private Timer _timer;
        private long _lastSent;
        private long _lastReceived;
        private long _lastPacketsReceived;
        private long _lastPacketsSent;

        public BandwidthTracker(NetManager manager, int timeInterval = 1000)
        {
            Manager = manager;
            _timer = new Timer(OnTimerTick, null, 0, timeInterval);
        }

        private void OnTimerTick(object state)
        {
            var statistics = Manager.Statistics;
            if (statistics != null)
            {
                BytesSent = (int)(statistics.BytesSent - _lastSent);
                BytesReceived = (int)(statistics.BytesReceived - _lastReceived);
                PacketsSent = (int) (statistics.PacketsSent - _lastPacketsSent);
                PacketsReceived = (int)(statistics.PacketsReceived - _lastPacketsReceived);

                _lastSent = statistics.BytesSent;
                _lastReceived = statistics.BytesReceived;
                _lastPacketsReceived = statistics.PacketsReceived;
                _lastPacketsSent = statistics.PacketsSent;
            }
        }
    }
}
