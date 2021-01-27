using System;

namespace Riders.Netplay.Messages
{
    public interface IPacket : IDisposable
    {
        /// <summary>
        /// Gets the type of packet this individual packet corresponds to.
        /// </summary>
        PacketKind GetPacketType();

        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        byte[] Serialize();

        /// <summary>
        /// Deserializes data into the current packet..
        /// </summary>
        unsafe void Deserialize(Span<byte> data);
    }

    public interface IPacket<out T> where T : IPacket, new()
    {
        /// <summary>
        /// Deserializes and returns a new packet instance.
        /// </summary>
        static unsafe T FromSpan(Span<byte> data)
        {
            var result = new T();
            result.Deserialize(data);
            return result;
        }
    }

    public enum PacketKind
    {
        Reliable,
        Unreliable
    }
}