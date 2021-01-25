using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;
using Sewer56.SonicRiders.API;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public unsafe struct LapCounters : IBitPackedArray<LapCounter, LapCounters>
    {
        public  const int NumberOfEntries       = Constants.MaxNumberOfPlayers; // Note: Spectators will need host information too, hence max players not max peers.
        private const int SizeOfEntryBits       = LapCounter.SizeOfEntryBits;
        private const int SizeOfAllEntriesBytes = (((SizeOfEntryBits * NumberOfEntries) + SizeOfAllEntriesMod) / 8);
        private const int SizeOfAllEntriesMod   = (SizeOfEntryBits * NumberOfEntries) % 8;

        private fixed byte _data[SizeOfAllEntriesBytes];

        /// <inheritdoc />
        public IBitPackedArray<LapCounter, LapCounters> AsInterface() => this;

        /// <inheritdoc />
        public int GetBufferSize() => SizeOfAllEntriesBytes;

        /// <inheritdoc />
        public ref byte GetFixedBuffer() => ref _data[0];
    }
}