using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
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
        private const string NtpServer = "0.pool.ntp.org";
        private const int NtpSyncEventPeriod = 16000;
        private const int FirstNtpSyncDueTime = 4000;

        /// <inheritdoc />
        public Socket Socket { get; set; }
        public NetManager Manager { get; set; }
        private Timer _synchronizeTimer { get; set; }
        private TimeSpan _correctionOffset = TimeSpan.Zero;
        private bool _receivedNtpResponse = true;

        public TimeSynchronization(Socket socket)
        {
            Socket = socket;
            Manager = socket.Manager;
            socket.Listener.NtpResponseEvent += OnNtpResponse;
            _synchronizeTimer = new Timer(RequestNtpSynchronize, null, FirstNtpSyncDueTime, NtpSyncEventPeriod);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _synchronizeTimer.Dispose();
        }

        /// <summary>
        /// Converts local time to server time.
        /// </summary>
        public DateTime ToServerTime(DateTime time) => time + _correctionOffset;

        /// <summary>
        /// Converts server time to server time.
        /// </summary>
        public DateTime ToLocalTime(DateTime time) => time - _correctionOffset;

        private void OnNtpResponse(NtpPacket packet)
        {
            if (packet != null)
            {
                Trace.WriteLine($"[{nameof(TimeSynchronization)}] NTP Time Synchronized, Offset: {packet.CorrectionOffset.TotalMilliseconds}ms");
                _correctionOffset = packet.CorrectionOffset;
                _receivedNtpResponse = true;
            }
        }

        private void RequestNtpSynchronize(object? state)
        {
            if (_receivedNtpResponse)
            {
                _receivedNtpResponse = false;
                Manager.CreateNtpRequest(NtpServer);
                Socket.PollUntil(() => _receivedNtpResponse == true, 0, 0);
            }
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet) { }
    }
}
