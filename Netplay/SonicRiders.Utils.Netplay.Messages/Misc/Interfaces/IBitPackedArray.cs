using System;
using System.Buffers;
using System.IO;
using BitStreams;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages.Misc.Interfaces
{
    /// <summary>
    /// Specifies an interface that allows for bit packing of arrays of unmanaged structs smaller than a byte.
    /// </summary>
    public unsafe interface IBitPackedArray<T, TParent> : IDisposable
                                                          where T : unmanaged, IBitPackable<T>
                                                          where TParent : IBitPackedArray<T, TParent>, new()
    {
        public static ArrayPool<T> SharedPool        = ArrayPool<T>.Shared;

        /// <summary>
        /// The data to be packed/unpacked.
        /// </summary>
        public T[] Elements { get; set; }

        /// <summary>
        /// The number of elements in the <see cref="Elements"/> array.
        /// </summary>
        public int NumElements { get; set; }

        /// <summary>
        /// True if memory for the elements is borrowed from the <see cref="System.Buffers.ArrayPool{T}"/>, else false.
        /// Internal use only.
        /// </summary>
        public bool IsPooled { get; set; }

        /// <summary>
        /// Gets the expected size of header and all entries in bytes once serialized.
        /// </summary>
        public int SizeOfDataBytes => (Utilities.RoundUp(ItemCountNumBits + (SizeOfEntryBits * NumElements), 8) / 8);

        /// <summary>
        /// Disposes of the array.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (IsPooled)
                SharedPool.Return(Elements);
        }

        /// <summary>
        /// Number of bits allocated for the number of items in the array.
        /// </summary>
        public int ItemCountNumBits => Constants.PlayerCountBitfield.NumBits;

        /// <summary>
        /// Creates a new instance of the structure.
        /// </summary>
        public IBitPackedArray<T, TParent> AsInterface();

        /// <summary>
        /// Creates an instance of the parent using the specified elements.
        /// Note: Array must at least contain 1 element.
        /// </summary>
        public TParent Create(T[] elements)
        {
#if DEBUG
            // Restricted to debug because exceptions prevent inlining.
            if (elements.Length == 0)
                throw new Exception("Array has zero elements.");
#endif

            return new TParent
            {
                IsPooled = false,
                Elements = elements,
                NumElements = elements.Length
            };
        }

        /// <summary>
        /// Deserializes the serialized packed array.
        /// </summary>
        /// <param name="reader">Stream reader.</param>
        /// <returns></returns>
        public TParent Deserialize(BufferedStreamReader reader)
        {
            var parent  = new TParent {IsPooled = true};
            var child   = new T();
            var numBits = parent.ItemCountNumBits;

            // Setup the bitstream
            var stream      = reader.BaseStream();
            var originalPos = reader.Position();
            stream.Position = originalPos;
            var bitStream   = new BitStream(stream);

            // Read the stream
            parent.NumElements = bitStream.Read<int>(numBits) + 1;
            parent.Elements    = SharedPool.Rent(parent.NumElements);

            for (int x = 0; x < parent.NumElements; x++)
                parent.Elements[x] = child.FromStream(bitStream);

            // Finalize the bitstream.
            var bitStreamPos = bitStream.GetStream().Position;
            var extraByte = bitStream.BitPosition != 0 ? 1 : 0;
            reader.Seek(originalPos + bitStreamPos + extraByte, SeekOrigin.Begin);

            return parent;
        }

        /// <summary>
        /// Serializes the current array.
        /// </summary>
        public Span<byte> Serialize(Span<byte> resultBuffer)
        {
            // Setup
            fixed (byte* bytePtr = resultBuffer)
            {
                var memStream = new UnmanagedMemoryStream(bytePtr, resultBuffer.Length, resultBuffer.Length, FileAccess.ReadWrite);
                var bitStream = new BitStream(memStream);
                bitStream.AutoIncreaseStream = true;

                // Write the stream
                bitStream.Write(NumElements - 1, ItemCountNumBits);
                for (int x = 0; x < NumElements; x++)
                    Elements[x].ToStream(bitStream);

                // Cleanup
                memStream.Seek(0, SeekOrigin.Begin);
                bitStream.CopyStreamTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                var result = resultBuffer.Slice(0, SizeOfDataBytes);
                memStream.Read(result);

                // Dispose
                memStream.Dispose();
                return result;
            }
        }

        /// <summary>
        /// Serializes the current array.
        /// </summary>
        public byte[] Serialize()
        {
            var result = new byte[SizeOfDataBytes];
            Serialize(result.AsSpan());
            return result;
        }

        /// <summary>
        /// Gets the size of an individual entry.
        /// </summary>
        private int SizeOfEntryBits => new T().GetSizeOfEntry();
    }
}