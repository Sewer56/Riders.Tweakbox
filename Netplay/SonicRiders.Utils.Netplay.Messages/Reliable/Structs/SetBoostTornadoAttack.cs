using Riders.Netplay.Messages.Reliable.Structs.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs
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