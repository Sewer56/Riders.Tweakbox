using Riders.Netplay.Messages.Helpers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct EndNetplayGame : IReliableMessage
    {
        public EndMode Mode;

        public EndNetplayGame(EndMode mode) => Mode = mode;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public MessageType GetMessageType() => MessageType.EndGame;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            Mode = bitStream.ReadGeneric<EndMode>(EnumNumBits<EndMode>.Number);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteGeneric<EndMode>(Mode, EnumNumBits<EndMode>.Number);
        }
    }

    public enum EndMode
    {
        Exit,
        Restart
    }
}