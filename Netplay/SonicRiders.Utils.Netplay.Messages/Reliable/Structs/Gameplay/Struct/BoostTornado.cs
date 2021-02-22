using EnumsNET;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using static Sewer56.SonicRiders.Structures.Enums.MovementFlags;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct
{
    /// <summary>
    /// Sent from Client to Host and then Host to Other Clients when a client's Boost or Tornado become activated.
    /// </summary>
    public struct BoostTornado : Misc.Interfaces.IBitPackable<BoostTornado>, IMergeable<BoostTornado>
    {
        /// <summary>
        /// The flag.
        /// </summary>
        public BoostTornadoFlags Flag;

        /// <param name="player">The player to get flags from.</param>
        public unsafe BoostTornado(Player* player) : this(player->MovementFlags, player->LastMovementFlags) { }

        /// <summary>
        /// Create movement flags based on player's existing flags.
        /// </summary>
        public BoostTornado(Sewer56.SonicRiders.Structures.Enums.MovementFlags flags, Sewer56.SonicRiders.Structures.Enums.MovementFlags lastFlags)
        {
            Flag = BoostTornadoFlags.None;
            if (flags.HasAllFlags(Boosting) && !lastFlags.HasAllFlags(Boosting))
                Flag |= BoostTornadoFlags.Boost;

            if (flags.HasAllFlags(Tornado) && !lastFlags.HasAllFlags(Tornado))
                Flag |= BoostTornadoFlags.Tornado;
        }

        /// <summary>
        /// True if this struct has a value, else false.
        /// </summary>
        public bool HasValue() => Flag != BoostTornadoFlags.None;

        /// <summary>
        /// Applies the movement flags to a given player by appending the flags to the new movement flags for this frame.
        /// </summary>
        /// <param name="player">Pointer to the player to apply the flags to.</param>
        public unsafe void ToGame(Player* player)
        {
            if (Flag.HasAllFlags(BoostTornadoFlags.Boost))
                player->MovementFlags |= Boosting;

            if (Flag.HasAllFlags(BoostTornadoFlags.Tornado))
                player->MovementFlags |= Tornado;
        }

        /// <summary>
        /// Applies the movement flags to a given player by appending the flags to the new movement flags for this frame.
        /// </summary>
        /// <param name="player">Pointer to the player to apply the flags to.</param>
        public unsafe void ToGameAndReset(Player* player)
        {
            ToGame(player);
            Flag = BoostTornadoFlags.None;
        }

        /// <summary>
        /// Merges a new set of flags with the current set.
        /// </summary>
        public void Merge(in BoostTornado other)
        {
            this.Flag |= other.Flag;
        }

        /// <inheritdoc />
        public BoostTornado FromStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            return new BoostTornado()
            {
                Flag = stream.ReadGeneric<BoostTornadoFlags>(EnumNumBits<BoostTornadoFlags>.Number)
            };
        }

        /// <inheritdoc />
        public void ToStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            stream.WriteGeneric(Flag, EnumNumBits<BoostTornadoFlags>.Number);
        }
    }
}