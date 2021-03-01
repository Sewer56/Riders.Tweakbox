using System.Collections.Generic;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Struct
{
    [Equals(DoNotAddEqualityOperators = true)]
    public class PlayerData : IReusable
    {
        public const int NumPlayersBits = 2;

        /// <summary>
        /// Time within which to remember the highest latency.
        /// </summary>
        public const int MaxLatencyTimeMs = 15000;

        /// <summary>
        /// Time between which latency updates are expected.
        /// </summary>
        public const int LatencyUpdatePeriod = 1000;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name;

        /// <summary>
        /// Index of the individual player.
        /// This corresponds to the indices in <see cref="UnreliablePacket"/>.
        /// Ignore if received from client.
        /// </summary>
        public int PlayerIndex;

        /// <summary>
        /// Contains the current ping of the individual player.
        /// </summary>
        public int Latency;

        /// <summary>
        /// Contains the recent latencies of the player.
        /// This is latencies which happened within the last <see cref="MaxLatencyTimeMs"/>
        /// </summary>
        public List<Timestamped<int>> RecentLatencies = new List<Timestamped<int>>((MaxLatencyTimeMs / LatencyUpdatePeriod) + 1);

        /// <summary>
        /// Number of local players assigned to this machine.
        /// </summary>
        public int NumPlayers;

        /// <summary>
        /// Copies data submitted by the client.
        /// </summary>
        public void UpdateFromClient(PlayerData data)
        {
            this.Name       = data.Name;
            this.NumPlayers = data.NumPlayers;
        }

        /// <summary>
        /// Updates the latency for this client.
        /// </summary>
        public void UpdateLatency(int latency)
        {
            Latency = latency;
            RecentLatencies.Add(latency);

            for (int x = RecentLatencies.Count - 1; x >= 0; x--)
            {
                if (RecentLatencies[x].IsDiscard(MaxLatencyTimeMs))
                    RecentLatencies.RemoveAt(x);
            }
        }

        public unsafe void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            Name        = bitStream.ReadString();
            PlayerIndex = bitStream.Read<int>(Constants.MaxNumberOfClientsBitField.NumBits);
            Latency     = bitStream.Read<int>(NumPlayersBits);
            NumPlayers  = bitStream.Read<int>(NumPlayersBits);
        }

        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteString(Name);
            bitStream.Write(PlayerIndex, Constants.MaxNumberOfClientsBitField.NumBits);
            bitStream.Write(Latency, NumPlayersBits);
            bitStream.Write(NumPlayers, NumPlayersBits);
        }

        /// <inheritdoc />
        public void Reset()
        {
            Name = null;
            PlayerIndex = -1;
            Latency = 999;
            NumPlayers = 1;
        }
    }
}
