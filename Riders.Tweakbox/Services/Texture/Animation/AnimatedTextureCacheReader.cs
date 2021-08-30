using Riders.Tweakbox.Misc.Pointers;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Riders.Tweakbox.Services.Texture.Animation
{
    /// <summary>
    /// Reads an animated texture cache file.
    /// </summary>
    public unsafe struct AnimatedTextureCacheReader : IDisposable
    {
        private byte[] _data;

        private GCHandle _handle;
        private int* _currentPointer;
        private int* _firstFilePointer;

        /// <summary>
        /// The version of this archive file.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// The file count for this archive file.
        /// </summary>
        public int FileCount { get; private set; }

        /// <summary>
        /// Creates an instance of an animated texture cache reader.
        /// </summary>
        /// <param name="data">The data to read the cache contents from.</param>
        public AnimatedTextureCacheReader(byte[] data)
        {
            _data = data;
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _currentPointer = (int*)_handle.AddrOfPinnedObject();

            // Read Header
            Version   = *_currentPointer;
            FileCount = *(_currentPointer + 1);
            _currentPointer += 2;
            _firstFilePointer = _currentPointer;
        }

        /// <summary>
        /// Disposes the cache reader.
        /// </summary>
        public void Dispose() => _handle.Free();

        /// <summary>
        /// Resets the file iterator to its original position.
        /// </summary>
        public void Reset() => _currentPointer = _firstFilePointer;

        /// <summary>
        /// Retrieves the pointers to all of the files in the archive.
        /// Note: Spans should have <see cref="FileCount"/> elements.
        /// </summary>
        /// <param name="sizes">Sizes of each file.</param>
        /// <param name="dataPointers">Pointer to each file's data.</param>
        /// <returns>True if file is available, else false.</returns>
        public void GetAllFiles(Span<TextureCacheTuple> tuples)
        {
            var originalPointer = _currentPointer;
            Reset();

            int index = 0;
            while (TryGetNextFile(out int size, out byte* dataPtr))
            {
                tuples[index++] = new TextureCacheTuple()
                {
                    DataPtr = dataPtr, 
                    Size = size
                };
            }

            _currentPointer = originalPointer;
        }

        /// <summary>
        /// Tries to get the next file from unmanaged memory.
        /// </summary>
        /// <param name="size">Size of the file.</param>
        /// <param name="dataPtr">Pointer to the file data.</param>
        /// <returns>True if file is available, else false.</returns>
        public bool TryGetNextFile(out int size, out byte* dataPtr)
        {
            dataPtr = default;
            size = *_currentPointer;

            // End of File
            if (size == AnimatedTextureCacheWriter.EndOfFile)
                return false;

            // Else get data and go to next.
            dataPtr = (byte*)(_currentPointer + 1);
            _currentPointer = (int*)(dataPtr + size);
            return true;
        }
    }
    public unsafe struct TextureCacheTuple
    {
        public int Size;
        public byte* DataPtr;
    }
}