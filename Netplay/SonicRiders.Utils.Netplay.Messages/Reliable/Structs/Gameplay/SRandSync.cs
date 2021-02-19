using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct SRandSync : IReliableMessage
    {
        /// <summary>
        /// The Date/Time to resume the game at, synced to an external FTP server.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// The seed to apply to the clients.
        /// </summary>
        public int Seed;
        
        public SRandSync(DateTime startTime, int seed)
        {
            StartTime = startTime;
            Seed = seed;
        }

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.SRand;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            StartTime = bitStream.ReadGeneric<DateTime>();
            Seed = bitStream.Read<int>();
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteGeneric(StartTime);
            bitStream.Write(Seed);
        }
    }
}
