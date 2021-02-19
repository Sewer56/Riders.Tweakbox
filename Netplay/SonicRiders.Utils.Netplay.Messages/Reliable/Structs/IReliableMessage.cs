using System;
using Riders.Netplay.Messages.Misc.Interfaces;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs
{
    /// <summary>
    /// Abstracts a message that is infrequently sent (less than 1 time per frame).
    /// </summary>
    public interface IReliableMessage : IDisposable
    {
        /// <summary>
        /// Returns the type associated with this message.
        /// </summary>
        MessageType GetMessageType();

        /// <summary>
        /// Deserializes the contents of this message from a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream;

        /// <summary>
        /// Serializes the contents of this message to a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream;
    }
}