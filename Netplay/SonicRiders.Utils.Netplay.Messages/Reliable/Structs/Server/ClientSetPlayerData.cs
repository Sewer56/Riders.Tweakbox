using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Server;

[Equals(DoNotAddEqualityOperators = true)]
public struct ClientSetPlayerData : IReliableMessage
{
    public ClientData Data { get; set; }

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    readonly MessageType IReliableMessage.GetMessageType() => MessageType.ClientSetPlayerData;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Data ??= new ClientData();
        Data.FromStream(ref bitStream);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Data.ToStream(ref bitStream);
    }
}
