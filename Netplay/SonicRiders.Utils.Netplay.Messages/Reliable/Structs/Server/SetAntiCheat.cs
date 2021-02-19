using System;
using Reloaded.Memory;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server
{
    /// <summary>
    /// Sets the Anti-cheat types for this server.
    /// </summary>
    public struct SetAntiCheat : IReliableMessage
    {
        public CheatKind Cheats { get; set; }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteGeneric(Cheats, EnumNumBits<CheatKind>.Number);
        }

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            Cheats = bitStream.ReadGeneric<CheatKind>(EnumNumBits<CheatKind>.Number);
        }

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        readonly MessageType IReliableMessage.GetMessageType() => MessageType.SetAntiCheatTypes;
    }
}
