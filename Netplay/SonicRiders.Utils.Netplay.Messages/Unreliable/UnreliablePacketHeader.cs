using System;
using EnumsNET;
using Reloaded.Memory;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public struct UnreliablePacketHeader
    {
        public const int    MaxNumPlayers = 32;
        private const short NumPlayersMask = 0x001F;
        private const int   NumPlayersBits = 5;
        
        /*
           // Header (2 bytes)
           Data Bitfields    = 11 bits
           Number of Players = 5 bits 
        */

        /// <summary>
        /// Declares the fields present in the packet to be serialized/deserialized.
        /// </summary>
        public HasData Fields { get; private set; }

        /// <summary>
        /// The number of player entries stored in this unreliable message.
        /// </summary>
        public byte NumberOfPlayers { get; private set; }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="players">List of players to include in the packet.</param>
        public UnreliablePacketHeader(UnreliablePacketPlayer[] players)
        {
            if (players.Length < 1 || players.Length > MaxNumPlayers)
                throw new Exception($"Number of players must be in the range 1-{MaxNumPlayers}.");

            NumberOfPlayers = (byte)players.Length;
            Fields = HasData.All;
        }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="players">List of players to include in the packet.</param>
        /// <param name="data">The data to include in the packet.</param>
        public UnreliablePacketHeader(UnreliablePacketPlayer[] players, HasData data = HasData.All) : this(players)
        {
            Fields = data;
        }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="players">List of players to include in the packet.</param>
        /// <param name="frameCounter">The current frame counter used to determine if data should be sent or not.</param>
        public UnreliablePacketHeader(UnreliablePacketPlayer[] players, int frameCounter) : this(players)
        {
            Fields = GetData(frameCounter);
        }

        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        public unsafe byte[] Serialize()
        {
            // f: Fields, n: Numbers
            // ffff ffff ffff fnnn
            ushort fieldsPacked = (ushort)((ushort)Fields << NumPlayersBits);
            byte numPlayersPacked = (byte)(NumberOfPlayers - 1);

            ushort message = (ushort)((ushort)fieldsPacked | (ushort)numPlayersPacked);
            return Struct.GetBytes(message);
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static UnreliablePacketHeader Deserialize(BufferedStreamReader reader)
        {
            // f: Fields, n: Numbers
            // ffff ffff ffff fnnn
            reader.Read(out ushort message);
            byte numberOfPlayers = (byte)((byte)(message & NumPlayersMask) + 1);
            var fields = message >> NumPlayersBits;

            return new UnreliablePacketHeader
            {
                NumberOfPlayers = numberOfPlayers,
                Fields = (HasData)fields
            };
        }

        /// <summary>
        /// Determines if on a given frame, a piece of data should be sent.
        /// </summary>
        /// <param name="frameCounter">The current frame counter.</param>
        /// <param name="type">The type of data.</param>
        /// <returns>True if should be sent, else false.</returns>
        private static bool ShouldISend(int frameCounter, HasData type)
        {
            // Send every freq frame.
            bool ShouldISendFrequency(int frame, int freq) => frame % freq == 0;

            // State is currently disabled, seems to be redundant since we synced animations.
            if (type == HasData.HasState)
                return false;

            switch (type)
            {
                case HasData.HasRings:
                    return ShouldISendFrequency(frameCounter, 6);

                case HasData.HasAir:
                    return ShouldISendFrequency(frameCounter, 12);

                case HasData.HasTurnAndLean:
                    return ShouldISendFrequency(frameCounter, 3);

                case HasData.HasControlFlags:
                    return ShouldISendFrequency(frameCounter, 3);
            }

            // Used to have settings here, removed for now.
            // Will be implemented if we ever need to reduce bandwidth usage.
            return true;
        }

        /// <summary>
        /// Determines the information to be sent.
        /// Only use if available bandwidth is constrained.
        /// </summary>
        /// <param name="frameCounter">The current frame counter.</param>
        public static HasData GetData(int frameCounter)
        {
            var hasData = HasData.Null;
            if (ShouldISend(frameCounter, HasData.HasPosition)) hasData |= HasData.HasPosition;
            if (ShouldISend(frameCounter, HasData.HasRotation)) hasData |= HasData.HasRotation;
            if (ShouldISend(frameCounter, HasData.HasVelocity)) hasData |= HasData.HasVelocity;
            if (ShouldISend(frameCounter, HasData.HasRings)) hasData |= HasData.HasRings;
            if (ShouldISend(frameCounter, HasData.HasState)) hasData |= HasData.HasState;
            if (ShouldISend(frameCounter, HasData.HasAir)) hasData |= HasData.HasAir;
            if (ShouldISend(frameCounter, HasData.HasTurnAndLean)) hasData |= HasData.HasTurnAndLean;
            if (ShouldISend(frameCounter, HasData.HasControlFlags)) hasData |= HasData.HasControlFlags;
            if (ShouldISend(frameCounter, HasData.HasAnimation)) hasData |= HasData.HasAnimation;
            if (ShouldISend(frameCounter, HasData.HasUnused5)) hasData |= HasData.HasUnused5;
            if (ShouldISend(frameCounter, HasData.HasUnused6)) hasData |= HasData.HasUnused6;

            return hasData;
        }

        /// <summary>
        /// Declares whether the packet has a particular component of data.
        /// </summary>
        [Flags]
        public enum HasData : ushort
        {
            All                = HasPosition | HasRotation | HasVelocity | HasRings | HasState | HasAir | HasTurnAndLean | HasControlFlags | HasAnimation | HasUnused5 | HasUnused6,
            Null               = 0,
            
            HasPosition        = 1 << 0, 
            HasRotation        = 1 << 1, 
            HasVelocity        = 1 << 2, 
            HasRings           = 1 << 3, 
            HasState           = 1 << 4,
            HasAir             = 1 << 5,
            HasTurnAndLean     = 1 << 6, 
            HasControlFlags    = 1 << 7, 
            HasAnimation       = 1 << 8, 
            HasUnused5         = 1 << 9, 
            HasUnused6         = 1 << 10, 
            // Last 5 bytes occupied by player count.
        }
    }
}