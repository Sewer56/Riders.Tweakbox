namespace Riders.Netplay.Messages
{
    /// <summary>
    /// A wrapper for all packet types.
    /// </summary>
    public class Packet
    {
        public ReliablePacket?   Reliable;
        public UnreliablePacket? Unreliable;

        public Packet(ReliablePacket? reliable, UnreliablePacket? unreliable)
        {
            Reliable = reliable;
            Unreliable = unreliable;
        }
    }
}
