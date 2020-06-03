namespace Riders.Netplay.Messages.Reliable.Structs
{
    public struct AntiCheatTriggered
    {
        /// <summary>
        /// Player who triggered the alert.
        /// </summary>
        public byte PlayerIndex;

        /// <summary>
        /// The supposed cheat that triggered the alert.
        /// </summary>
        public CheatKind Cheat;

        public AntiCheatTriggered(byte playerIndex, CheatKind cheat)
        {
            PlayerIndex = playerIndex;
            Cheat = cheat;
        }

        /// <summary>
        /// The type of cheat that was detected.
        /// </summary>
        public enum CheatKind : byte
        {
            None,
            Speedhack,
            Teleport,
            ModifiedStats,
            LapCounter,
            DriftFrameCounter,
            BoostFrameCounter,

            // TODO: Implement after initial release
            RngManipulation, // Call rand, then again, check for same value. (Then restore)
        }
    }
}
