using System;
using System.Buffers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.Interfaces
{
    /// <summary>
    /// Specifies an interface that allows for bit packing of arrays of unmanaged structs smaller than a byte.
    /// </summary>
    public unsafe interface IBitPackedArray<T, TParent> : IDisposable
                                                          where T : unmanaged, IBitPackable<T>
                                                          where TParent : struct, IBitPackedArray<T, TParent>
    {
        public static ArrayPool<T> SharedPool = ArrayPool<T>.Shared;

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
        /// Number of bits allocated for the number of items in the array.
        /// </summary>
        public int ItemCountNumBits => Constants.PlayerCountBitfield.NumBits;

        /// <summary>
        /// Disposes of the array.
        /// </summary>
        public static void Dispose(ref TParent type)
        {
            if (type.IsPooled)
            {
                SharedPool.Return(type.Elements);
                type.Elements = null;
            }
        }

        /// <summary>
        /// Replaces the internal array with a new set of elements.
        /// </summary>
        public void Set(T[] elements, int numElements = -1);

        /// <summary>
        /// Creates an instance of the parent using the specified elements.
        /// Note: Array must at least contain 1 element.
        /// </summary>
        public TParent Create(T[] elements);

        /// <summary>
        /// Creates a pooled instance of the parent using the specified elements.
        /// Note: Array must at least contain 1 element.
        /// </summary>
        public TParent CreatePooled(int numElements);

        /// <summary>
        /// Deserializes the serialized packed array.
        /// </summary>
        public static TParent FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            var parent  = new TParent {IsPooled = true};
            var child   = new T();
            var numBits = parent.ItemCountNumBits;

            // Read the stream
            parent.NumElements = bitStream.Read<int>(numBits) + 1;
            parent.Elements    = SharedPool.Rent(parent.NumElements);

            for (int x = 0; x < parent.NumElements; x++)
                parent.Elements[x] = child.FromStream(ref bitStream);

            return parent;
        }

        /// <summary>
        /// Serializes the current array.
        /// </summary>
        public static void ToStream<TByteSource>(in TParent parent, ref BitStream<TByteSource> bitStream) where TByteSource : IByteStream
        {
            // Write the stream
            bitStream.Write(parent.NumElements - 1, parent.ItemCountNumBits);
            for (int x = 0; x < parent.NumElements; x++)
                parent.Elements[x].ToStream(ref bitStream);
        }
    }
}