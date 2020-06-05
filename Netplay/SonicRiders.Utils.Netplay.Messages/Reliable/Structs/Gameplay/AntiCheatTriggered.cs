using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
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
    }
}
