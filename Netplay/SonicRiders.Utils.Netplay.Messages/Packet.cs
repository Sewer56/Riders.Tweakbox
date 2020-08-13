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
        /// Returns the <see cref="PacketKind"/> associated with this packet. (e.g. Reliable/Unreliable)
        /// </summary>
        public PacketKind GetPacketKind() => Value.Value.GetPacketType();

        /// <summary>
        /// Converts the internal timestamped packet value into a given packet format.
        /// </summary>
        public T As<T>() where T : IPacket => (T)Value.Value;

        /// <summary>
        /// Attempts to get the packet message if the packet is not discarded based on timeout and matches the given packet kind/type.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="packet">The packet itself.</param>
        public bool TryGetPacket<T>(int timeout, out T packet) where T : IPacket, new()
        {
            packet = new T();
            if (Value.IsDiscard(timeout))
                return false;

            if (GetPacketKind() != packet.GetPacketType())
                return false;

            packet = As<T>();
            return true;
        }
    }
}
