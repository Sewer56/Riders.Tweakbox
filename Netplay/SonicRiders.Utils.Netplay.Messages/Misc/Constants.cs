using System.ComponentModel;
using Sewer56.SonicRiders.Utility;

namespace Riders.Netplay.Messages.Misc
{
    /// <summary>
    /// Various constants related to message sending.
    /// </summary>
    public static class Constants
    {
        // TODO: 32-Player Support | Replace APIs: GetPlayerIndex, Player.Players

        /// <summary>
        /// Maximum number of local players playing on one PC.
        /// </summary>
        public const int MaxNumberOfLocalPlayers = 4;

        /// <summary>
        /// The maximum number of clients that can connect to a server.
        /// </summary>
        public static int MaxNumberOfClients => MaxNumberOfClientsBitField.MaxValue + 1;

        /// <summary>
        /// [Bitfields] The maximum number of clients that can connect to a server.
        /// </summary>
        public static BitField MaxNumberOfClientsBitField { get; private set; } = new BitField(6);

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
        public static BitField PlayerCountBitfield { get; private set; } = new BitField(3);

        /// <summary>
        /// Internal use only.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetPlayerCountBitfield(BitField value)
        {
            PlayerCountBitfield = value;
        }
    }
}
