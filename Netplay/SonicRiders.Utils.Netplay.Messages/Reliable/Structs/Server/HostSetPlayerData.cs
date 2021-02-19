using System;
using System.Buffers;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server
{
    public struct HostSetPlayerData : IReliableMessage
    {
        private static ArrayPool<PlayerData> _pool = ArrayPool<PlayerData>.Shared;

        /// <summary>
        /// Contains indexes and names of all other players.
        /// </summary>
        public PlayerData[] Data { get; set; }

        /// <summary>
        /// Index of the player receiving the message.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Number of elements in the <see cref="Data"/> array.
        /// </summary>
        public int NumElements { get; private set; }

        private bool _isPooled;

        public HostSetPlayerData(PlayerData[] data, int index)
        {
            Data = data;
            Index = index;

            NumElements = Data.Length;
            _isPooled = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isPooled)
            {
                _pool.Return(Data);
            }
        }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.HostSetPlayerData;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            // Our own created elements use the pool.
            _isPooled = true;

            NumElements = bitStream.Read<byte>(Constants.MaxNumberOfClientsBitField.NumBits) + 1;
            Index = bitStream.Read<byte>(Constants.PlayerCountBitfield.NumBits);
            Data  = _pool.Rent(NumElements);
            for (int x = 0; x < NumElements; x++)
            {
                var data = new PlayerData();
                data.FromStream(ref bitStream);
                Data[x] = data;
            }
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(NumElements - 1, Constants.MaxNumberOfClientsBitField.NumBits);
            bitStream.Write(Index, Constants.PlayerCountBitfield.NumBits);

            for (int x = 0; x < NumElements; x++) 
                Data[x].ToStream(ref bitStream);
        }
    }
}
