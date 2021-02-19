namespace Riders.Netplay.Messages
{
    /// <summary>
    /// A wrapper for all packet types.
    /// </summary>
    public struct Packet<TSource, TPacketType>
    {
        public TSource Source;
        public TPacketType Value;

        public Packet(TSource source, TPacketType value)
        {
            Source = source;
            Value  = value;
        }
    }
}