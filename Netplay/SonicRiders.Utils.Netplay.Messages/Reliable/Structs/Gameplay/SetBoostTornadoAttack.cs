using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Informs the host when performing boost, tornado, attack.
    /// </summary>
    public struct SetBoostTornadoAttack
    {
        /// <summary>
        /// Any combination of Boost, Tornado, Attack.
        /// </summary>
        public AttackModes Modes;
    }
}