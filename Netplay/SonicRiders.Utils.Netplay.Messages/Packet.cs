namespace Riders.Netplay.Messages
{
    /// <summary>
    /// A wrapper for all packet types.
    /// </summary>
    public class Packet<T>
    {
        public T Source;
        public ReliablePacket?   Reliable;
        public UnreliablePacket? Unreliable;

        public Packet(T source, ReliablePacket? reliable, UnreliablePacket? unreliable)
        {
            Source = source;
            Reliable = reliable;
            Unreliable = unreliable;
        }
    }
}
