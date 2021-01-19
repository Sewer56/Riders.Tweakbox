using System;
using System.IO;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages.Misc
{
    /// <summary>
    /// Wrapper functions for LZ4 operations.
    /// </summary>
    public static class LZ4
    {
        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        /// <param name="expectedSize">Expected size of the compressed data. An accurate guess results in better performance.</param>
        public static unsafe byte[] CompressLZ4Stream(Stream source, LZ4Level level = LZ4Level.L10_OPT, int expectedSize = 1000)
        {
            using var targetStream = new MemoryStream(expectedSize);
            using var compStream = LZ4Stream.Encode(targetStream, level, 0, true);

            source.CopyTo(compStream);
            compStream.Close();
            return targetStream.ToArray();
        }

        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        /// <param name="expectedSize">Expected size of the compressed data. An accurate guess results in better performance.</param>
        public static unsafe byte[] CompressLZ4Stream(Span<byte> source, LZ4Level level = LZ4Level.L10_OPT, int expectedSize = 1000)
        {
            fixed (byte* bytePtr = &source[0])
            {
                using var sourceStream = new UnmanagedMemoryStream(bytePtr, source.Length);
                return CompressLZ4Stream(sourceStream, level, expectedSize);
            }
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="buffer">Preallocated buffer of expected size for data.</param>
        /// <param name="stream">Stream to decompress data from.</param>
        /// <returns>The supplied buffer.</returns>
        public static byte[] DecompressLZ4Stream(byte[] buffer, Stream stream)
        {
            // Move stream to read position / Reloaded.Memory does not sync base stream offset.
            using var targetStream = new MemoryStream(buffer, true);
            using var decompStream = LZ4Stream.Decode(stream, 0, true);
            decompStream.CopyTo(targetStream);
            return buffer;
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="stream">Stream to decompress data from.</param>
        /// <param name="expectedSize">Expected size of the deserialized data. An accurate guess results in better performance.</param>
        /// <returns>The supplied buffer.</returns>
        public static byte[] DecompressLZ4Stream(Stream stream, int expectedSize = 1000)
        {
            // Move stream to read position / Reloaded.Memory does not sync base stream offset.
            using var targetStream = new MemoryStream(expectedSize);
            using var decompStream = LZ4Stream.Decode(stream, 0, true);
            decompStream.CopyTo(targetStream);
            return targetStream.ToArray();
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="buffer">Preallocated buffer of expected size for data.</param>
        /// <param name="source">Source to decompress data from.</param>
        /// <param name="numBytesRead">Number of bytes read by the stream.</param>
        /// <returns>The supplied buffer.</returns>
        public static unsafe byte[] DecompressLZ4Stream(byte[] buffer, Span<byte> source, out int numBytesRead)
        {
            fixed (byte* ptr = source)
            {
                using var sourceStream = new UnmanagedMemoryStream(ptr, source.Length);
                DecompressLZ4Stream(buffer, sourceStream);
                numBytesRead = (int)sourceStream.Position;
                return buffer;
            }
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="expectedSize">Expected size of decompressed data.  An accurate guess results in better performance.</param>
        /// <param name="source">Source to decompress data from.</param>
        /// <param name="numBytesRead">Number of bytes read by the stream.</param>
        /// <returns>The supplied buffer.</returns>
        public static unsafe byte[] DecompressLZ4Stream(int expectedSize, Span<byte> source, out int numBytesRead)
        {
            fixed (byte* ptr = source)
            {
                using var sourceStream = new UnmanagedMemoryStream(ptr, source.Length);
                var decompressed = DecompressLZ4Stream(sourceStream, expectedSize);
                numBytesRead = (int)sourceStream.Position;
                return decompressed;
            }
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="buffer">Preallocated buffer of expected size for data.</param>
        /// <param name="reader">Stream to decompress data from.</param>
        /// <returns>The supplied buffer.</returns>
        public static byte[] DecompressLZ4Stream(byte[] buffer, BufferedStreamReader reader)
        {
            // Move stream to read position / Reloaded.Memory does not sync base stream offset. 
            var baseStream = reader.BaseStream();
            baseStream.Position = reader.Position();

            var result = DecompressLZ4Stream(buffer, baseStream);
            reader.Seek(baseStream.Position, SeekOrigin.Begin);
            return result;
        }
    }
}
