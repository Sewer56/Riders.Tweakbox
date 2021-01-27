using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;
using Sewer56.SonicRiders.API;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public unsafe struct LapCounters : IBitPackedArray<LapCounter, LapCounters>
    {
        /// <inheritdoc />
        public LapCounter[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public IBitPackedArray<LapCounter, LapCounters> AsInterface() => this;
    }
}