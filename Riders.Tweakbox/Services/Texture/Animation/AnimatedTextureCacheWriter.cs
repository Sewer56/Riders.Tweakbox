using System;
using System.IO;
using Reloaded.Memory.Streams;

namespace Riders.Tweakbox.Services.Texture.Animation
{
    /// <summary>
    /// Writes an animated texture cache file.
    /// </summary>
    public struct AnimatedTextureCacheWriter : IDisposable
    {
        /*
            Cache Format:
            
            int size;
            byte data[size];
        
            For all files until size == -1; 
        */

        /// <summary>
        /// Constant size that marks the end of file.
        /// </summary>
        public const int EndOfFile = -1;

        /// <summary>
        /// Current version of the cache file.
        /// </summary>
        public const int CurrentVersion = 1;

        private MemoryStream _stream;
        private int _fileCount;
        private int _numFiles;

        /// <summary>
        /// Initinalizes a cache writer given an estimate complete file size.
        /// </summary>
        /// <param name="estimatedSize">Estimated total size of the data.</param>
        /// <param name="numFiles">The number of files that will be contained in this archive.</param>
        public AnimatedTextureCacheWriter(int estimatedSize, int numFiles)
        {
            _stream = new MemoryStream(estimatedSize);
            _numFiles = numFiles;
            _fileCount = 0;

            // Write Header
            _stream.Write<int>(CurrentVersion);
            _stream.Write<int>(_numFiles);
        }

        public void Dispose() => _stream?.Dispose();

        /// <summary>
        /// Adds a file to the stream.
        /// </summary>
        public bool TryWriteFile(Span<byte> data)
        {
            bool success = _fileCount < _numFiles;
            if (success)
            {
                _stream.Write<int>(data.Length);
                _stream.Write(data);
                _fileCount++;
            }

            return success;
        }

        /// <summary>
        /// Writes the end file marker to the stream.
        /// No file should be added after this marker.
        /// </summary>
        public void Finish() => _stream.Write(EndOfFile);

        /// <summary>
        /// Retreives the underlying buffer as a span.
        /// </summary>
        /// <returns></returns>
        public Span<byte> GetSpan() => _stream.GetBuffer().AsSpan(0, (int)_stream.Length);
    }
}