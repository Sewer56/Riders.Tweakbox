using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public unsafe struct LapCountersPacked : IReliableMessage, IBitPackedArray<LapCounter, LapCountersPacked>
    {
        /// <inheritdoc />
        public LapCounter[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.LapCounters;

        /// <inheritdoc />
        public void Set(LapCounter[] elements, int numElements = -1) => this.Set<LapCountersPacked, LapCounter, LapCountersPacked>(elements, numElements);

        /// <inheritdoc />
        public LapCountersPacked Create(LapCounter[] elements) => this.Create<LapCountersPacked, LapCounter, LapCountersPacked>(elements);

        /// <inheritdoc />
        public LapCountersPacked CreatePooled(int numElements) => this.CreatePooled<LapCountersPacked, LapCounter, LapCountersPacked>(numElements);
        public void ToPooled(int numElements) => this = CreatePooled(numElements);

        /// <inheritdoc />
        public void Dispose() => IBitPackedArray<LapCounter, LapCountersPacked>.Dispose(ref this);

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            this.Dispose();
            this = IBitPackedArray<LapCounter, LapCountersPacked>.FromStream(ref bitStream);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => IBitPackedArray<LapCounter, LapCountersPacked>.ToStream(this, ref bitStream);
    }
}