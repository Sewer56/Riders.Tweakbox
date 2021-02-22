using System.Buffers;
using Riders.Netplay.Messages.Misc;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server
{
    [Equals(DoNotAddEqualityOperators = true)]
    public struct HostUpdateClientLatency : IReliableMessage
    {
        private static ArrayPool<short> _pool = ArrayPool<short>.Shared;
        private const int LatencyNumBits = 10;

        /// <summary>
        /// Contains latency numbers for each client.
        /// </summary>
        public short[] Data { get; set; }

        /// <summary>
        /// Number of elements in the <see cref="Data"/> array.
        /// </summary>
        public int NumElements { get; private set; }

        private bool _isPooled;

        public HostUpdateClientLatency(short[] data)
        {
            Data = data;
            _isPooled = false;
            NumElements = data.Length;
        }

        /// <inheritdoc />
        readonly MessageType IReliableMessage.GetMessageType() => MessageType.HostUpdateClientPing;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isPooled)
            {
                _pool.Return(Data);
            }
        }

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            _isPooled = true;

            NumElements = bitStream.Read<byte>(Constants.MaxNumberOfClientsBitField.NumBits) + 1;
            Data = _pool.Rent(NumElements);
            for (int x = 0; x < NumElements; x++)
                Data[x] = bitStream.Read<short>(LatencyNumBits);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(NumElements - 1, Constants.MaxNumberOfClientsBitField.NumBits);
            for (int x = 0; x < NumElements; x++)
                bitStream.Write(Data[x], LatencyNumBits);
        }
    }
}