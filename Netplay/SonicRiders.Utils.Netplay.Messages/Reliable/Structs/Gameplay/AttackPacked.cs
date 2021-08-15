using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay;

/// <summary>
/// Message from host to client that communicates a series of attacks to be performed.
/// </summary>
public unsafe struct AttackPacked : IReliableMessage, IBitPackedArray<SetAttack, AttackPacked>
{
    /// <inheritdoc />
    public SetAttack[] Elements { get; set; }

    /// <inheritdoc />
    public int NumElements { get; set; }

    /// <inheritdoc />
    public bool IsPooled { get; set; }

    /// <inheritdoc />
    public void Set(SetAttack[] elements, int numElements = -1) => this.Set<AttackPacked, SetAttack, AttackPacked>(elements, numElements);

    /// <inheritdoc />
    public AttackPacked Create(SetAttack[] elements) => this.Create<AttackPacked, SetAttack, AttackPacked>(elements);

    /// <inheritdoc />
    public AttackPacked CreatePooled(int numElements) => this.CreatePooled<AttackPacked, SetAttack, AttackPacked>(numElements);
    public void ToPooled(int numElements) => this = CreatePooled(numElements);

    /// <inheritdoc />
    public void Dispose() => IBitPackedArray<SetAttack, AttackPacked>.Dispose(ref this);

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.Attack;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Dispose();
        this = IBitPackedArray<SetAttack, AttackPacked>.FromStream(ref bitStream);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => IBitPackedArray<SetAttack, AttackPacked>.ToStream(this, ref bitStream);
}
