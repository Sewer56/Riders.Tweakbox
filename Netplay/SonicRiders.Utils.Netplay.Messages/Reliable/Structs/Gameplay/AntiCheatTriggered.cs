using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct AntiCheatTriggered : IReliableMessage
    {
        /// <summary>
        /// Player who triggered the alert.
        /// </summary>
        public byte PlayerIndex;

        /// <summary>
        /// The supposed cheat that triggered the alert.
        /// </summary>
        public CheatKind Cheat;

        public AntiCheatTriggered(byte playerIndex, CheatKind cheat)
        {
            PlayerIndex = playerIndex;
            Cheat = cheat;
        }

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.AntiCheatTriggered;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            PlayerIndex = bitStream.Read<byte>(Constants.PlayerCountBitfield.NumBits);
            Cheat = bitStream.ReadGeneric<CheatKind>(EnumNumBits<CheatKind>.Number);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(PlayerIndex, Constants.PlayerCountBitfield.NumBits);
            bitStream.WriteGeneric(Cheat, EnumNumBits<CheatKind>.Number);
        }
    }
}
