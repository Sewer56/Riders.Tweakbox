using System;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Utility;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public struct UnreliablePacketHeader
    {
        public static readonly BitField SequenceNumberBitfield = new BitField(7);

        /// <summary>
        /// Sequence number assigned to this packet.
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// The number of player entries stored in this unreliable message.
        /// </summary>
        public byte NumberOfPlayers { get; private set; }

        /// <summary>
        /// Declares the fields present in the packet to be serialized/deserialized.
        /// </summary>
        public HasData Fields { get; private set; }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="numPlayers">Number of players in this message.</param>
        /// <param name="sequenceNumber">Individual sequence number assigned to this packet.</param>
        public UnreliablePacketHeader(byte numPlayers, int sequenceNumber)
        {
#if DEBUG
            if (numPlayers < 1 || numPlayers > Constants.MaxNumberOfPlayers)
                throw new Exception($"Number of players must be in the range 1-{Constants.MaxNumberOfPlayers}.");
#endif

            NumberOfPlayers = numPlayers;
            Fields = HasDataAll;
            SequenceNumber = (int) (sequenceNumber % (SequenceNumberBitfield.MaxValue + 1));
        }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="numPlayers">Number of players in this message.</param>
        /// <param name="sequenceNumber">Individual sequence number assigned to this packet.</param>
        /// <param name="data">The data to include in the packet.</param>
        public UnreliablePacketHeader(byte numPlayers, int sequenceNumber, HasData data = HasDataAll) : this(numPlayers, sequenceNumber)
        {
            Fields = data;
        }

        /// <summary>
        /// Creates a packet header given a list of players to include in the packet.
        /// </summary>
        /// <param name="numPlayers">Number of players in this message.</param>
        /// <param name="sequenceNumber">Individual sequence number assigned to this packet.</param>
        /// <param name="frameCounter">The current frame counter used to determine if data should be sent or not.</param>
        public UnreliablePacketHeader(byte numPlayers, int sequenceNumber, int frameCounter) : this(numPlayers, sequenceNumber)
        {
            Fields = GetData(frameCounter);
        }

        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        public unsafe void Serialize<TByteStream>(ref BitStream<TByteStream> stream) where TByteStream : IByteStream
        {
            stream.Write(SequenceNumber, SequenceNumberBitfield.NumBits);
            stream.Write(NumberOfPlayers - 1, Constants.PlayerCountBitfield.NumBits);
            stream.WriteGeneric(Fields, EnumNumBits<HasData>.Number);
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static UnreliablePacketHeader Deserialize<TByteStream>(ref BitStream<TByteStream> stream) where TByteStream : IByteStream
        {
            return new UnreliablePacketHeader
            {
                SequenceNumber = stream.Read<byte>(SequenceNumberBitfield.NumBits),
                NumberOfPlayers = (byte)(stream.Read<byte>(Constants.PlayerCountBitfield.NumBits) + 1),
                Fields = stream.ReadGeneric<HasData>(EnumNumBits<HasData>.Number)
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

            return type switch
            {
                HasData.HasRings => ShouldISendFrequency(frameCounter, 6),
                HasData.HasAir => ShouldISendFrequency(frameCounter, 12),
                HasData.HasTurnAndLean => ShouldISendFrequency(frameCounter, 3),
                HasData.HasMovementFlags => ShouldISendFrequency(frameCounter, 3),
                HasData.HasAnalogInput => ShouldISendFrequency(frameCounter, 3),

                // Used to have settings here, removed for now.
                // Will be implemented if we ever need to reduce bandwidth usage.
                _ => true
            };
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
            if (ShouldISend(frameCounter, HasData.HasMovementFlags)) hasData |= HasData.HasMovementFlags;
            if (ShouldISend(frameCounter, HasData.HasAnalogInput)) hasData |= HasData.HasAnalogInput;

            return hasData;
        }

        /// <summary>
        /// Declares whether the packet has a particular component of data.
        /// </summary>
        [Flags]
        public enum HasData : int
        {
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
            HasMovementFlags   = 1 << 9,
            HasAnalogInput     = 1 << 10,
        }

        /// <summary>
        /// All items of <see cref="HasData"/> enum.
        /// Please DO NOT put me inside <see cref="HasData"/> as it breaks the code that auto-determines number of bits. 
        /// </summary>
        public const HasData HasDataAll = HasData.HasPosition | HasData.HasRotation | HasData.HasVelocity | HasData.HasRings | HasData.HasState | 
                                          HasData.HasAir | HasData.HasTurnAndLean | HasData.HasControlFlags | HasData.HasAnimation | HasData.HasMovementFlags |
                                          HasData.HasAnalogInput;
    }
}