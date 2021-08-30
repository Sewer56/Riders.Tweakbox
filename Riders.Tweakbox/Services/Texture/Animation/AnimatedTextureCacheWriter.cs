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
        private MemoryStream _stream;

        /// <summary>
        /// Initinalizes a cache writer given an estimate complete file size.
        /// </summary>
        /// <param name="estimatedSize">Estimated total size of the data.</param>
        public AnimatedTextureCacheWriter(int estimatedSize)
        {
            _stream = new MemoryStream(estimatedSize);
        }

        public void Dispose() => _stream?.Dispose();

        /// <summary>
        /// Adds a file to the stream.
        /// </summary>
        /// <param name="data"></param>
        public void AddFile(Span<byte> data)
        {
            _stream.Write<int>(data.Length);
            _stream.Write(data);
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