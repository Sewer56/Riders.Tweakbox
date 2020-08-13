using System.Threading;
using LiteNetLib;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Tracks bandwidth over course of time for a given network manager.
    /// </summary>
    public class BandwidthTracker
    {
        public int BytesSent     { get; private set; }
        public int BytesReceived { get; private set; }
        public float KBytesSent =>     BytesSent / 1000.0f;
        public float KBytesReceived => BytesReceived / 1000.0f;

        public NetManager Manager;
        private Timer _timer;
        private ulong _lastSent;
        private ulong _lastReceived;

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

                _lastSent = statistics.BytesSent;
                _lastReceived = statistics.BytesReceived;
            }
        }
    }
}
