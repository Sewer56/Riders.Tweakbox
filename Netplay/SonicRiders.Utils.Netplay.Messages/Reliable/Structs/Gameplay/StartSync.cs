using Riders.Netplay.Messages.Helpers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct StartSync : IReliableMessage
    {
        public StartSyncType SyncType;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.StartSync;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            SyncType = bitStream.ReadGeneric<StartSyncType>(EnumNumBits<StartSyncType>.Number);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteGeneric(SyncType, EnumNumBits<StartSyncType>.Number);
        }
    }

    public enum StartSyncType
    {
        Null,

        /// <summary>
        /// Informs client/host that the cutscene should be skipped.
        /// </summary>
        Skip,

        /// <summary>
        /// If received from host, indicates the player is ready to skip.
        /// If received from a client, add to list of ready clients.
        /// </summary>
        Ready
    }
}