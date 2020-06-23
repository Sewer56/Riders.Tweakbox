using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    /// <summary>
    /// Message from host to client that communicates a series of attacks to be performed.
    /// </summary>
    public unsafe struct AttackPacked : IBitPackedArray<SetAttack, AttackPacked>
    {
        public const int  NumberOfEntries = Constants.MaxNumberOfPlayers;
        private const int SizeOfEntryBits = SetAttack.SizeOfEntryBits;
        private const int SizeOfAllEntriesBytes = ((SizeOfEntryBits * NumberOfEntries) / 8);

        private fixed byte _data[SizeOfAllEntriesBytes];

        /// <inheritdoc />
        public IBitPackedArray<SetAttack, AttackPacked> AsInterface() => this;

        /// <inheritdoc />
        public int GetBufferSize() => SizeOfAllEntriesBytes;

        /// <inheritdoc />
        public ref byte GetFixedBuffer() => ref _data[0];
    }
}