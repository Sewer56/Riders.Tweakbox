using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using DotNext;
using DotNext.Buffers;
using K4os.Compression.LZ4;

namespace Riders.Tweakbox.Services.Texture
{
    public class TextureCompression
    {
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
            var compressedData = GC.AllocateUninitializedArray<byte>(numEncoded);
            fileStream.Read(compressedData.AsSpan());

            var uncompressedData = GC.AllocateUninitializedArray<byte>(numUncompressed);
            LZ4Codec.Decode(compressedData.AsSpan(), uncompressedData.AsSpan());
            return new TextureRef(uncompressedData);
        }
    }
}
