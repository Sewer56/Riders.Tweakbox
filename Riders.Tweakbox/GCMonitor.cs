using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Riders.Tweakbox.Misc;

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
                    switch (status)
                    {
                        case GCNotificationStatus.Succeeded:
                            Log.WriteLine($"[Full GC] About to Begin GC, Mode: {GCSettings.LatencyMode}", LogCategory.Memory);
                            break;
                        case GCNotificationStatus.Canceled:
                            Log.WriteLine("[Full GC] Begin Event was Cancelled", LogCategory.Memory);
                            break;
                    }

                    // Check for a notification of a completed collection.
                    status = GC.WaitForFullGCComplete(-1);
                    switch (status)
                    {
                        case GCNotificationStatus.Succeeded:
                            Log.WriteLine($"[Full GC] Completed", LogCategory.Memory);
                            break;
                        case GCNotificationStatus.Canceled:
                            Log.WriteLine($"[Full GC] Completion was cancelled.", LogCategory.Memory);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLine("[Full GC] Logger has encountered an error.", LogCategory.Memory);
                Log.WriteLine("[Full GC] " + e.Message, LogCategory.Memory);
            }
        }
    }
}
