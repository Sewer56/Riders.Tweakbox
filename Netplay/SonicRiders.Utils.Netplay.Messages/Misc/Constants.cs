﻿using Sewer56.SonicRiders.Utility;

namespace Riders.Netplay.Messages.Misc
{
    public static class Constants
    {
        // TODO: 32-Player Support | Replace APIs: GetPlayerIndex, Player.Players

        /// <summary>
        /// Maximum number of players supported by Sonic Riders.
        /// The use of this variable indicates code/game needs adjusting for extra players.
        /// Hint: 32-player mode in the future.
        /// </summary>
        public static int MaxRidersNumberOfPlayers => 8;

        /// <summary>
        /// Maximum number of players supported by the Netplay mod (indexed from 1).
        /// </summary>
        public static int MaxNumberOfPlayers => PlayerCountBitfield.MaxValue + 1;

        /// <summary>
        /// Provides information about the bitfield used for storing player counts in various messages.
        /// </summary>
        public static readonly BitField PlayerCountBitfield = new BitField(5);
    }
}
