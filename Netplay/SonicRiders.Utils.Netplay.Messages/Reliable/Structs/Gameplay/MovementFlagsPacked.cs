using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Message from the host that movement flags such as boost & tornado for clients.
    /// </summary>
    public unsafe struct MovementFlagsPacked : IBitPackedArray<MovementFlagsMsg, MovementFlagsPacked>
    {
        public const int NumberOfEntries        = Constants.MaxNumberOfPeers;
        private const int SizeOfEntryBits       = MovementFlagsMsg.SizeOfEntry;
        private const int SizeOfAllEntriesBytes = (((SizeOfEntryBits * NumberOfEntries) + SizeOfAllEntriesMod) / 8);
        private const int SizeOfAllEntriesMod   = (SizeOfEntryBits * NumberOfEntries) % 8;

        private fixed byte _data[SizeOfAllEntriesBytes];

        /// <inheritdoc />
        public IBitPackedArray<MovementFlagsMsg, MovementFlagsPacked> AsInterface() => this;

        /// <inheritdoc />
        public int GetBufferSize() => SizeOfAllEntriesBytes;

        /// <inheritdoc />
        public ref byte GetFixedBuffer() => ref _data[0];
    }
}