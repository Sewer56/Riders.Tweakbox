using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Message from host to client that communicates a series of attacks to be performed.
    /// </summary>
    public unsafe struct AttackPacked : IBitPackedArray<SetAttack, AttackPacked>
    {
        /// <inheritdoc />
        public SetAttack[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public IBitPackedArray<SetAttack, AttackPacked> AsInterface() => this;
    }
}