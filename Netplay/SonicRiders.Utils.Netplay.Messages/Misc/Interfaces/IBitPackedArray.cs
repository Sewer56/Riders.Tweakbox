using System.IO;
using BitStreams;

namespace Riders.Netplay.Messages.Misc.Interfaces
{
    /// <summary>
    /// Specifies an interface that allows for bit packing of arrays of unmanaged structs smaller than a byte.
    /// </summary>
    public unsafe interface IBitPackedArray<T, TParent> where T : unmanaged, IBitPackable<T>
                                                        where TParent : unmanaged, IBitPackedArray<T, TParent>
    {
        /// <summary>
        /// Creates a new instance of the structure.
        /// </summary>
        public IBitPackedArray<T, TParent> AsInterface();

        /// <summary>
        /// Gets the size of the fixed buffer in bytes.
        /// </summary>
        int GetBufferSize();

        /// <summary>
        /// Returns the address of the internal fixed buffer.
        /// </summary>
        ref byte GetFixedBuffer();

        /// <summary>
        /// Retrieves the offset of an individual item with a specific index.
        /// </summary>
        int GetOffset(int index) => new T().GetSizeOfEntry() * index;

        /// <summary>
        /// Gets the data for an individual item.
        /// </summary>
        /// <param name="index">Index of data to retrieve.</param>
        public T GetData(int index)
        {
            var initialOffset = GetOffset(index);
            fixed (byte* buffer = &GetFixedBuffer())
            {
                using var memStream = new UnmanagedMemoryStream((byte*)buffer, GetBufferSize());
                var bitStream = new BitStream(memStream);

                bitStream.Seek(0, initialOffset);
                return new T().FromStream(bitStream);
            }
        }

        /// <summary>
        /// Sets the data of an individual item.
        /// </summary>
        /// <param name="data">The data to set.</param>
        /// <param name="index">Index of data to set.</param>
        public TParent SetData(T data, int index)
        {
            var initialOffset   = GetOffset(index);
            fixed (byte* buffer = &GetFixedBuffer())
            {
                // Copy original stream.
                using var memStream = new UnmanagedMemoryStream(buffer, GetBufferSize(), GetBufferSize(), FileAccess.ReadWrite);
                var bitStream = new BitStream(memStream);

                // Get to and write player data.
                bitStream.Seek(0, initialOffset);
                data.ToStream(bitStream);

                // Copy back to original stream.
                memStream.Seek(0, SeekOrigin.Begin);
                bitStream.CopyStreamTo(memStream);
            }

            return (TParent) this;
        }

        /// <summary>
        /// Sets a collection of items.
        /// </summary>
        /// <param name="data">The data to set.</param>
        /// <param name="offset">Index of first entry to set.</param>
        public TParent SetData(T[] data, int offset = 0)
        {
            for (int x = 0; x < data.Length; x++)
                SetData(data[x], x + offset);

            return (TParent)this;
        }

        /// <summary>
        /// Converts the internal contents to an array.
        /// </summary>
        /// <param name="buffer">The buffer to place the items in.</param>
        /// <param name="numStructs">Number of structures to unpack.</param>
        /// <param name="offset">Offset of the first item to get.</param>
        /// <param name="targetOffset">Offset to start putting items into the provided array.</param>
        public void ToArray(T[] buffer, int numStructs, int offset = 0, int targetOffset = 0)
        {
            for (int x = 0; x < numStructs; x++)
                buffer[x + targetOffset] = GetData(x + offset);
        }

        /// <summary>
        /// Creates a packed struct given an input array.
        /// </summary>
        /// <param name="flags">The flags to create the struct from.</param>
        /// <param name="offset">Offset of first item inside the result struct.</param>
        public static TParent FromArray(T[] flags, int offset = 0)
        {
            var packed = new TParent();
            packed.SetData(flags, offset);
            return packed;
        }
    }
}