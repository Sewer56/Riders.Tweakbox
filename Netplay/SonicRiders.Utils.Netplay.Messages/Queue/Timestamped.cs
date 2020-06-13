using System;
using System.Collections.Generic;
using System.Text;
using Riders.Netplay.Messages.Misc;

namespace Riders.Netplay.Messages.Queue
{
    /// <summary>
    /// Represents an individual timestamped packet/value.
    /// </summary>
    public class Timestamped<T>
    {
        public DateTime TimeStamp;
        public T Value;

        public Timestamped()
        {
            TimeStamp = DateTime.UtcNow;
        }

        public Timestamped(T value)
        {
            TimeStamp = DateTime.UtcNow;
            Value = value;
        }

        public Timestamped(DateTime timeStamp, T value)
        {
            TimeStamp = timeStamp;
            Value = value;
        }

        /// <summary>
        /// Checks if a packet should be discarded based on comparing the arrival and current time.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public bool IsDiscard(int timeout) => DateTime.UtcNow > TimeStamp.AddMilliseconds(timeout);

        public static implicit operator Timestamped<T> (T d) => new Timestamped<T>(d);
        public static implicit operator T (Timestamped<T> d) => d.Value;
    }
}
