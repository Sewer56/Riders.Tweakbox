using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct BoostTornadoPacked : IReliableMessage, IBitPackedArray<BoostTornado, BoostTornadoPacked>
    {
        /// <inheritdoc />
        public BoostTornado[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public void Set(BoostTornado[] elements, int numElements = -1) => this.Set<BoostTornadoPacked, BoostTornado, BoostTornadoPacked>(elements, numElements);

        /// <inheritdoc />
        public BoostTornadoPacked Create(BoostTornado[] elements) => this.Create<BoostTornadoPacked, BoostTornado, BoostTornadoPacked>(elements);

        /// <inheritdoc />
        public BoostTornadoPacked CreatePooled(int numElements) => this.CreatePooled<BoostTornadoPacked, BoostTornado, BoostTornadoPacked>(numElements);
        public void ToPooled(int numElements) => this = CreatePooled(numElements);

        /// <inheritdoc />
        public void Dispose() => IBitPackedArray<BoostTornado, BoostTornadoPacked>.Dispose(ref this);

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.BoostTornado;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            Dispose();
            this = IBitPackedArray<BoostTornado, BoostTornadoPacked>.FromStream(ref bitStream);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => IBitPackedArray<BoostTornado, BoostTornadoPacked>.ToStream(this, ref bitStream);

    }
}