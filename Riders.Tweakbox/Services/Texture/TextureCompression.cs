using System;
using System.IO;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;
using Reloaded.Memory.Streams;
using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture;

public class TextureCompression
{
    /// <summary>
    /// Decompresses a DDS.LZ4 from a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public static TextureRef PickleFromFileToRef(string filePath) => new TextureRef(PickleFromFile(filePath), TextureFormat.Dds);

    /// <summary>
    /// Decompresses a DDS.LZ4 from a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public static byte[] PickleFromFile(string filePath)
    {
        // Stack storage.
        Span<int> intSpan = stackalloc int[2];

        // Read Header
        using var fileStream = new FileStream(filePath, FileMode.Open);
        fileStream.Read(MemoryMarshal.AsBytes(intSpan));
        var numUncompressed = intSpan[1];
        var numEncoded = intSpan[0];

        // Read Data
        var compressedData = GC.AllocateUninitializedArray<byte>(numEncoded);
        fileStream.Read(compressedData.AsSpan());

        var uncompressedData = GC.AllocateUninitializedArray<byte>(numUncompressed);
        LZ4Codec.Decode(compressedData.AsSpan(), uncompressedData.AsSpan());
        return uncompressedData;
    }

    /// <summary>
    /// Writes a DDS.LZ4 to a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="data">The data to compress.</param>
    public static void PickleToFile(string filePath, Span<byte> data)
    {
        var compressedData = GC.AllocateUninitializedArray<byte>(data.Length);
        var encoded = LZ4Codec.Encode(data, compressedData, LZ4Level.L12_MAX);

        using var fileStream = new FileStream(filePath, FileMode.Create);

        // Write Compressed and Uncompressed.
        fileStream.Write<int>(encoded);
        fileStream.Write<int>(data.Length);
        fileStream.Write(compressedData.AsSpan().Slice(0, encoded));
    }
}
