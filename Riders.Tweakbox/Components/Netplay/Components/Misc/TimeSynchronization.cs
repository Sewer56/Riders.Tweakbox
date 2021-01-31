using System;
using System.Diagnostics;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    /// <summary>
    /// Synchronizes time to a network server using the NTP protocol at a fixed interval.
    /// We use this to synchronize events in real time like race start.
    /// </summary>
    public class TimeSynchronization : INetplayComponent
    {
        private const string NtpServer = "time.facebook.com";
        private const int NtpSyncEventPeriod = 16000;

        /// <inheritdoc />
        public Socket Socket { get; set; }
        public NetManager Manager { get; set; }
        private Timer _synchronizeTimer { get; set; }
        private TimeSpan _correctionOffset = TimeSpan.Zero;

        public TimeSynchronization(Socket socket)
        {
            Socket = socket;
            Manager = socket.Manager;
            socket.Listener.NtpResponseEvent += OnNtpResponse;
            CreateNtpRequest(null);
            _synchronizeTimer = new Timer(CreateNtpRequest, null, 0, NtpSyncEventPeriod);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Socket.Listener.NtpResponseEvent -= OnNtpResponse;
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
                Log.WriteLine($"[{nameof(TimeSynchronization)}] NTP Time Synchronized, Offset: {packet.CorrectionOffset.TotalMilliseconds}ms", LogCategory.Ntp);
                _correctionOffset = packet.CorrectionOffset;
            }
        }

        private void CreateNtpRequest(object? state)
        {
            if (Manager.PendingNtpRequests > 0) 
                return;

            if (Debugger.IsAttached) 
                return;

            Log.WriteLine($"[{nameof(TimeSynchronization)}] Queuing NTP Synchronization", LogCategory.Ntp);
            Manager.CreateNtpRequest(NtpServer);
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet) { }
    }
}
