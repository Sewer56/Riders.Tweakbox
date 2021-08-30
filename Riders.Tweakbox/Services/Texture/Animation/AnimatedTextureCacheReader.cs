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

        /// <summary>
        /// Creates an instance of an animated texture cache reader.
        /// </summary>
        /// <param name="data">The data to read the cache contents from.</param>
        public AnimatedTextureCacheReader(byte[] data)
        {
            _data = data;
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _currentPointer = (int*)_handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Disposes the cache reader.
        /// </summary>
        public void Dispose() => _handle.Free();

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
}