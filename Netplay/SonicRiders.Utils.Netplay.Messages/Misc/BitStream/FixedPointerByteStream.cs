using Reloaded.Memory.Pointers;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.BitStream
{
    public struct FixedPointerByteStream : IByteStream
    {
        public RefFixedArrayPtr<byte> Pointer { get; private set; }
        public FixedPointerByteStream(RefFixedArrayPtr<byte> pointer) => Pointer = pointer;

        /// <inheritdoc />
        public byte Read(int index) => Pointer[index];

        /// <inheritdoc />
        public void Write(byte value, int index) => Pointer[index] = value;
    }
}