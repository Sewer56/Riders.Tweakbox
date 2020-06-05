using System.IO;
using BitStreams;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Message from the host that triggers boost, tornado & attack for clients.
    /// </summary>
    public unsafe struct BoostTornadoAttackPacked
    {
        private const int SizeOfEntryBits = 6;
        private const int SizeOfAllEntriesBytes = 6;

        private const int SizeOfModesBits       = 3;
        private const int SizeOfAttackIndexBits = 3;

        /*
            Internal representation of the data.
            3 bits | Has Boost, Tornado, Attack
            3 bits | Index 0-7, the player attacked.
        
            Total size: 42 bits (7 players) / 6 bytes.
        */
        private fixed byte _playerIds[SizeOfAllEntriesBytes];

        /// <summary>
        /// Gets the data for an individual player.
        /// </summary>
        /// <param name="index">Index of player 0-6.</param>
        public BoostTornadoAttack GetPlayerData(int index)
        {
            var initialOffset = GetPlayerOffset(index);
            fixed (byte* ids = _playerIds)
            {
                using var memStream = new UnmanagedMemoryStream(ids, SizeOfAllEntriesBytes);
                var bitStream = new BitStream(memStream);

                bitStream.Seek(0, initialOffset);
                var modes       = bitStream.ReadByte(SizeOfModesBits);
                var attackIndex = bitStream.ReadByte(SizeOfAttackIndexBits);

                return new BoostTornadoAttack((AttackModes) modes, attackIndex);
            }
        }

        /// <summary>
        /// Sets the data for an individual player.
        /// </summary>
        /// <param name="data">The data to set.</param>
        /// <param name="index">Index of player 0-6.</param>
        public void SetPlayerData(BoostTornadoAttack data, int index)
        {
            var initialOffset = GetPlayerOffset(index);
            fixed (byte* ids = _playerIds)
            {
                // Copy original stream.
                using var memStream = new UnmanagedMemoryStream(ids, SizeOfAllEntriesBytes, SizeOfAllEntriesBytes, FileAccess.ReadWrite);
                var bitStream = new BitStream(memStream);

                // Get to and write player data.
                bitStream.Seek(0, initialOffset);
                bitStream.WriteByte((byte) data.Modes, SizeOfModesBits);
                bitStream.WriteByte(data.TargetPlayer, SizeOfAttackIndexBits);

                // Copy back to original stream.
                memStream.Seek(0, SeekOrigin.Begin);
                bitStream.CopyStreamTo(memStream);
            }
        }

        /// <summary>
        /// Gets the data offset for an individual player.
        /// </summary>
        private int GetPlayerOffset(int player) => SizeOfEntryBits * player;
    }

    /// <summary>
    /// Full sized struct for an individual player.
    /// </summary>
    public struct BoostTornadoAttack
    {
        public AttackModes Modes;
        public byte TargetPlayer;

        /// <param name="modes">The modes of attack.</param>
        /// <param name="targetPlayer">The other player index attacked by this player. Ignore if attack flag is not set.</param>
        public BoostTornadoAttack(AttackModes modes, byte targetPlayer)
        {
            Modes = modes;
            TargetPlayer = targetPlayer;
        }
    }
}