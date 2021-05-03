using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using DotNext.Buffers;
using K4os.Compression.LZ4;

namespace Riders.Tweakbox.Services.Texture
{
    public class TextureCompression
    {
        private static ArrayPool<byte> _texturePool = ArrayPool<byte>.Create(16_777_345, 1);

        /// <summary>
        /// Reads a DDS.LZ4 from a given file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static TextureRef PickleFromFile(string filePath)
        {
            // Stack storage.
            Span<int> intSpan = stackalloc int[2];

            // Read Header
            using var fileStream = new FileStream(filePath, FileMode.Open);
            fileStream.Read(MemoryMarshal.AsBytes(intSpan));
            var numUncompressed = intSpan[1];
            var numEncoded      = intSpan[0];
            
            // Read Data
            using var compressedData = new ArrayRental<byte>(_texturePool, numEncoded, false);
            var uncompressedData = _texturePool.Rent(numUncompressed);

            fileStream.Read(compressedData.Span);
            LZ4Codec.Decode(compressedData.Span, uncompressedData);
            return new TextureRef(uncompressedData, _texturePool);
        }
    }
}
