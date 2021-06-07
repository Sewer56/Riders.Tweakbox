using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Game
{
    public struct ServerGameModifiers : IReliableMessage
    {
        public bool DisableTornadoes;
        public bool DisableAttacks;
        public bool AlwaysTurbulence;
        public bool DisableSmallTurbulence;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public MessageType GetMessageType() => MessageType.ServerGameModifiers;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            DisableTornadoes = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
            DisableAttacks = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
            AlwaysTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
            DisableSmallTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(Convert.ToByte(DisableTornadoes), 1);
            bitStream.Write(Convert.ToByte(DisableAttacks), 1);
            bitStream.Write(Convert.ToByte(AlwaysTurbulence), 1);
            bitStream.Write(Convert.ToByte(DisableSmallTurbulence), 1);
        }
    }
}
