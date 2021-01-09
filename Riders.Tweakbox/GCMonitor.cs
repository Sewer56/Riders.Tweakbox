using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Riders.Tweakbox
{
    /// <summary>
    /// Garbage collector monitoring for debug purposes.
    /// </summary>
    public class GCMonitor
    {
        public static GCMonitor Instance { get; } = new GCMonitor();
        private Thread _gcMonitorThread;

        private GCMonitor()
        {
            GC.RegisterForFullGCNotification(1, 1);
            _gcMonitorThread = new Thread(MonitorGC);
            _gcMonitorThread.Start();
        }

        private void MonitorGC()
        {
            try
            {
                while (true)
                {
                    // Check for a notification of an approaching collection.
                    var status = GC.WaitForFullGCApproach(-1);
                    if (status == GCNotificationStatus.Succeeded)
                    {
                        Trace.WriteLine($"[Full GC] About to Begin GC, Mode: {GCSettings.LatencyMode}");
                    }
                    else if (status == GCNotificationStatus.Canceled)
                    {
                        Trace.WriteLine("[Full GC] Begin Event was Cancelled");
                    }

                    // Check for a notification of a completed collection.
                    status = GC.WaitForFullGCComplete(-1);
                    if (status == GCNotificationStatus.Succeeded)
                    {
                        Trace.WriteLine($"[Full GC] Completed");
                    }
                    else if (status == GCNotificationStatus.Canceled)
                    {
                        Trace.WriteLine($"[Full GC] Completion was cancelled.");
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Full GC] Logger has encountered an error.");
                Trace.WriteLine("[Full GC] " + e.Message);
            }
        }
    }
}
