using System;
using DotNext.Buffers;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.BitStream
{
    public struct RentalByteStream : IByteStream, IDisposable
    {
        public ArrayRental<byte> Data;
        public RentalByteStream(ArrayRental<byte> data) { Data = data; }

        /// <inheritdoc />
        public byte Read(int index) => Data.Span[index];

        /// <inheritdoc />
        public void Write(byte value, int index) => Data.Span[index] = value;

        /// <inheritdoc />
        public void Dispose() => Data.Dispose();
    }
}