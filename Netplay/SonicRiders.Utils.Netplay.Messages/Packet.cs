using Riders.Netplay.Messages.Queue;

namespace Riders.Netplay.Messages
{
    /// <summary>
    /// A wrapper for all packet types.
    /// </summary>
    public class Packet<TSource>
    {
        public TSource Source;
        public Timestamped<IPacket> Value;

        public Packet(TSource source, IPacket value)
        {
            Source = source;
            Value = new Timestamped<IPacket>(value);
        }

        /// <summary>
        /// Checks if a packet should be discarded based on comparing the arrival and current time.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public bool IsDiscard(int timeout) => Value.IsDiscard(timeout);

        /// <summary>
        /// Converts the internal timestamped packet value into a given packet format.
        /// </summary>
        public T As<T>() where T : IPacket => (T)Value.Value;
    }
}
