using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riders.Netplay.Messages.Misc;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server;

[Equals(DoNotAddEqualityOperators = true)]
public struct ChatMessage : IReliableMessage
{
    /// <summary>
    /// Maximum length of the message.
    /// Hopefully small enough to prevent packet fragmentation.
    /// </summary>
    public const int MaxLength = 1000;

    /// <summary>
    /// The client index from which the message originates from.
    /// This index corresponds to the host's local index.
    /// </summary>
    public int SourceIndex;

    /// <summary>
    /// The message associated with this
    /// </summary>
    public string Message;

    public ChatMessage(int source, string message)
    {
        SourceIndex = source;
        Message = message;
    }

    public MessageType GetMessageType() => MessageType.ChatMessage;

    public void Dispose() { }

    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        SourceIndex = bitStream.Read<int>(Constants.MaxNumberOfClientsBitField.NumBits);
        Message = bitStream.ReadString(MaxLength);
    }

    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write<int>(SourceIndex, Constants.MaxNumberOfClientsBitField.NumBits);
        bitStream.WriteString(Message, MaxLength);
    }
}
