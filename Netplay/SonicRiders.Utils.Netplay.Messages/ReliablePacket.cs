using System;
using DotNext.Buffers;
using Reloaded.Memory.Pointers;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Reliable.Structs;
using Sewer56.BitStream;
namespace Riders.Netplay.Messages;

public unsafe struct ReliablePacket : IPacket
{
    /// <summary>
    /// Flags attacked to the original packet.
    /// </summary>
    public MessageType MessageType;

    /// <summary>
    /// Union of all messages that can be potentially attached to this packet.
    /// </summary>
    public IReliableMessage Message;

    /// <summary>
    /// Creates a new instance of a reliable packet.
    /// </summary>
    /// <typeparam name="T">Unmanaged type parameter.</typeparam>
    /// <param name="value">Value to insert into the packet.</param>
    public static ReliablePacket Create<T>(in T value) where T : IReliableMessage
    {
        var packet = new ReliablePacket();
        packet.SetMessage(value);
        return packet;
    }

    /// <summary>
    /// Returns the message associated with this packet.
    /// </summary>
    public T GetMessage<T>() where T : IReliableMessage
    {
        return (T)Message;
    }

    /// <summary>
    /// Sets the message stored in the current packet.
    /// </summary>
    /// <param name="value">The value to assign to this class.</param>
    /// <returns>Instance of this.</returns>
    public ReliablePacket SetMessage<T>(in T value) where T : IReliableMessage
    {
        Message = value;
        MessageType = value.GetMessageType();
        return this;
    }

    /// <inheritdoc />
    public void Dispose() => Message?.Dispose();

    /// <summary>
    /// Converts this message to an set of bytes.
    /// </summary>
    public ArrayRental<byte> Serialize(out int numBytes)
    {
        // Rent some bytes.
        var rental = new ArrayRental<byte>(8192);
        var rentalStream = new RentalByteStream(rental);
        var bitStream = new BitStream<RentalByteStream>(rentalStream);

        // Serialize the data.
        bitStream.WriteGeneric(MessageType, EnumNumBits<MessageType>.Number);
        Message.ToStream(ref bitStream);

        numBytes = bitStream.NextByteIndex;
        return rental;
    }

    /// <summary>
    /// Deserializes an instance of the packet.
    /// </summary>
    public unsafe void Deserialize(Span<byte> data)
    {
        fixed (byte* dataPtr = data)
        {
            var fixedArrayStream = new FixedPointerByteStream(new RefFixedArrayPtr<byte>(dataPtr, data.Length));
            var bitStream = new BitStream<FixedPointerByteStream>(fixedArrayStream);
            MessageType = bitStream.ReadGeneric<MessageType>(EnumNumBits<MessageType>.Number);

            Message = MessageType.Get();
            Message.FromStream(ref bitStream);
        }
    }
}
