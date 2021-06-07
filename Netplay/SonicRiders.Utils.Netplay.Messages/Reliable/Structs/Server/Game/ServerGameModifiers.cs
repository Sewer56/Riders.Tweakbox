using System;
using Riders.Netplay.Messages.Helpers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Parser.Layout.Objects.ItemBox;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Game
{
    public struct ServerGameModifiers : IReliableMessage
    {
        public bool DisableTornadoes;
        public bool DisableAttacks;
        public bool AlwaysTurbulence;
        public bool DisableSmallTurbulence;

        public bool ReplaceRing100Box;
        public ItemBoxAttribute Ring100Replacement;

        public bool ReplaceAirMaxBox;
        public ItemBoxAttribute AirMaxReplacement;

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

            ReplaceRing100Box = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
            Ring100Replacement = bitStream.ReadGeneric<ItemBoxAttribute>(EnumNumBits<ItemBoxAttribute>.Number);
            ReplaceAirMaxBox = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
            AirMaxReplacement = bitStream.ReadGeneric<ItemBoxAttribute>(EnumNumBits<ItemBoxAttribute>.Number);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(Convert.ToByte(DisableTornadoes), 1);
            bitStream.Write(Convert.ToByte(DisableAttacks), 1);
            bitStream.Write(Convert.ToByte(AlwaysTurbulence), 1);
            bitStream.Write(Convert.ToByte(DisableSmallTurbulence), 1);

            bitStream.WriteGeneric(ReplaceRing100Box, 1);
            bitStream.WriteGeneric(Ring100Replacement, EnumNumBits<ItemBoxAttribute>.Number);
            bitStream.WriteGeneric(ReplaceAirMaxBox, 1);
            bitStream.WriteGeneric(AirMaxReplacement, EnumNumBits<ItemBoxAttribute>.Number);
        }
    }
}
