using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay;

public struct PitExitPacked : IReliableMessage, IBitPackedArray<PitExit, PitExitPacked>
{
    /// <inheritdoc />
    public PitExit[] Elements { get; set; }

    /// <inheritdoc />
    public int NumElements { get; set; }

    /// <inheritdoc />
    public bool IsPooled { get; set; }

    /// <inheritdoc />
    public void Set(PitExit[] elements, int numElements = -1) => this.Set<PitExitPacked, PitExit, PitExitPacked>(elements, numElements);

    /// <inheritdoc />
    public PitExitPacked Create(PitExit[] elements) => this.Create<PitExitPacked, PitExit, PitExitPacked>(elements);

    /// <inheritdoc />
    public PitExitPacked CreatePooled(int numElements) => this.CreatePooled<PitExitPacked, PitExit, PitExitPacked>(numElements);

    public void ToPooled(int numElements) => this = CreatePooled(numElements);

    /// <inheritdoc />
    public void Dispose() => IBitPackedArray<PitExit, PitExitPacked>.Dispose(ref this);

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.PitExit;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Dispose();
        this = IBitPackedArray<PitExit, PitExitPacked>.FromStream(ref bitStream);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => IBitPackedArray<PitExit, PitExitPacked>.ToStream(this, ref bitStream);
}