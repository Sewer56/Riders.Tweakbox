using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Server;

public struct Disconnect : IReliableMessage
{
    /// <summary>
    /// Reason the client was disconnected.
    /// </summary>
    public string Reason;

    public Disconnect(string reason) => Reason = reason;

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public MessageType GetMessageType() => MessageType.Disconnect;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => Reason = bitStream.ReadString();

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => bitStream.WriteString(Reason);
}
