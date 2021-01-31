using System;
using EnumsNET;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Full sized struct for an individual player.
    /// </summary>
    public struct MovementFlagsMsg : IEquatable<MovementFlagsMsg>, Misc.Interfaces.IBitPackable<MovementFlagsMsg>
    {
        public const int SizeOfEntry = 10;
        public MovementFlags Modes;

        /// <param name="modes">The modes of attack.</param>
        public MovementFlagsMsg(MovementFlags modes) { Modes = modes; }

        /// <param name="player">The player to get flags from.</param>
        public unsafe MovementFlagsMsg(Player* player)
        {
            Modes = MovementFlags.None;
            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Boosting))
                Modes |= MovementFlags.Boost;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Braking))
                Modes |= MovementFlags.Braking;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.ChargingJump))
                Modes |= MovementFlags.ChargingJump;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Tornado))
                Modes |= MovementFlags.Tornado;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Drifting))
                Modes |= MovementFlags.Drifting;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.AttachToRail))
                Modes |= MovementFlags.AttachToRail;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Left))
                Modes |= MovementFlags.Left;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Right))
                Modes |= MovementFlags.Right;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Up))
                Modes |= MovementFlags.Up;

            if (player->NewMovementFlags.HasAllFlags(Sewer56.SonicRiders.Structures.Enums.MovementFlags.Down))
                Modes |= MovementFlags.Down;
        }

        /// <summary>
        /// Applies the movement flags to a given player by appending the flags to the new movement flags for this frame.
        /// </summary>
        /// <param name="player">Pointer to the player to apply the flags to.</param>
        public unsafe void ToGame(Player* player)
        {
            if (Modes.HasAllFlags(MovementFlags.Boost))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Boosting;

            if (Modes.HasAllFlags(MovementFlags.Braking))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Braking;

            if (Modes.HasAllFlags(MovementFlags.ChargingJump))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.ChargingJump;

            if (Modes.HasAllFlags(MovementFlags.Tornado))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Tornado;

            if (Modes.HasAllFlags(MovementFlags.Drifting))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Drifting;

            if (Modes.HasAllFlags(MovementFlags.AttachToRail))
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.AttachToRail;

            if (Modes.HasAllFlags(MovementFlags.Left))
            {
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Left;
                player->AnalogX = -100;
            }

            if (Modes.HasAllFlags(MovementFlags.Right))
            {
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Right;
                player->AnalogX = 100;
            }

            if (Modes.HasAllFlags(MovementFlags.Up))
            {
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Up;
                player->AnalogY = 100;
            }

            if (Modes.HasAllFlags(MovementFlags.Down))
            {
                player->NewMovementFlags |= Sewer56.SonicRiders.Structures.Enums.MovementFlags.Down;
                player->AnalogY = -100;
            }
        }

        /// <summary>
        /// Merges a new set of flags with the current set.
        /// </summary>
        /// <param name="packetSetMovementFlags"></param>
        public void Merge(MovementFlagsMsg packetSetMovementFlags)
        {
            this.Modes |= packetSetMovementFlags.Modes;
        }

        /// <inheritdoc />
        public int GetSizeOfEntry() => SizeOfEntry;

        /// <inheritdoc />
        public MovementFlagsMsg FromStream<T>(ref BitStream<T> stream) where T : IByteStream => new MovementFlagsMsg((MovementFlags)stream.Read<short>(SizeOfEntry));

        /// <inheritdoc />
        public void ToStream<T>(ref BitStream<T> stream) where T : IByteStream => stream.Write((short)Modes, SizeOfEntry);

        #region Autogenerated by R#
        /// <inheritdoc />
        public bool Equals(MovementFlagsMsg other) => Modes == other.Modes;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is MovementFlagsMsg other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine((int)Modes);
        #endregion
    }
}