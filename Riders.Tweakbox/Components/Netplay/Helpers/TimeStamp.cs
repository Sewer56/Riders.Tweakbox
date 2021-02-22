using System;

namespace Riders.Tweakbox.Components.Netplay.Helpers
{
    /// <summary>
    /// Represents an individual moment in time.
    /// </summary>
    public struct TimeStamp
    {
        public DateTime Stamp;

        /// <summary>
        /// Refreshes the timestamp associated with this struct to current time.
        /// </summary>
        public void Refresh() => Stamp = DateTime.UtcNow;

        /// <summary>
        /// Sets the packet time to the unix epoch time such that it's discarded for being too old.
        /// </summary>
        public void Discard() => Stamp = DateTime.UnixEpoch;

        /// <summary>
        /// Checks if a packet should be discarded based on comparing the arrival and current time.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public bool IsDiscard(int timeout) => DateTime.UtcNow > Stamp.AddMilliseconds(timeout);
    }
}