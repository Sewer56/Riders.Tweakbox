using System;
using DotNext.Buffers;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages
{
    public interface IPacket : IDisposable
    {
        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        /// <param name="numBytes">Number of bytes in the rental stream.</param>
        ArrayRental<byte> Serialize(out int numBytes);

        /// <summary>
        /// Deserializes data into the current packet..
        /// </summary>
        unsafe void Deserialize(Span<byte> data);
    }
}