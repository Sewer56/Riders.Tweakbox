using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Message from the host that movement flags such as boost & tornado for clients.
    /// </summary>
    public unsafe struct MovementFlagsPacked : IBitPackedArray<MovementFlagsMsg, MovementFlagsPacked>
    {
        /// <inheritdoc />
        public MovementFlagsMsg[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public IBitPackedArray<MovementFlagsMsg, MovementFlagsPacked> AsInterface() => this;
    }
}