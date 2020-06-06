using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct SyncStartGo
    {
        /// <summary>
        /// The Date/Time at which the Heartbeat was generated.
        /// </summary>
        public DateTime StartTime;

        public SyncStartGo(DateTime startTime) => StartTime = startTime;
        public static SyncStartGo FromCurrentTime() => new SyncStartGo(DateTime.UtcNow);
        public static SyncStartGo FromSoon(int milliseconds = 500) => new SyncStartGo(DateTime.UtcNow.AddMilliseconds(milliseconds));
    }
}
