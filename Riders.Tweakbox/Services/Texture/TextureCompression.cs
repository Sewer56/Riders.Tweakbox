using System;
using System.IO;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;
using Reloaded.Memory.Streams;
using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture;

public unsafe class TextureCompression
{
    /// <summary>
    /// Decompresses a DDS.LZ4 from a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public static TextureRef PickleFromFileToRef(string filePath) => new TextureRef(PickleFromFile(filePath), TextureFormat.Dds);

    /// <summary>
    /// Decompresses a DDS.LZ4 from a given file path.
    /// </summary>
    /// <param name="stream">The file path.</param>
    public static byte[] PickleFromMemory(byte* data, int dataLength)
    {
        // Read Header
        var numEncoded = *(int*)(data);
        var numUncompressed = *(int*)(data + 4);

        // Read Data
        var uncompressedData = GC.AllocateUninitializedArray<byte>(numUncompressed);
        LZ4Codec.Decode(new ReadOnlySpan<byte>(data + 8, dataLength - 8), uncompressedData.AsSpan());
        return uncompressedData;
    }

    /// <summary>
    /// Decompresses a DDS.LZ4 from a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public static byte[] PickleFromFile(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var fileData = GC.AllocateUninitializedArray<byte>((int)fileStream.Length);
        fileStream.Read(fileData.AsSpan());

        fixed (byte* dataPtr = fileData)
        {
            return PickleFromMemory(dataPtr, fileData.Length);
        }
    }

    /// <summary>
    /// Writes a DDS.LZ4 to an array.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <param name="level">The compression level.</param>
    public static Span<byte> PickleToArray(Span<byte> data, LZ4Level level = LZ4Level.L12_MAX)
    {
        const int headerSize = 8;
        var compressedData = GC.AllocateUninitializedArray<byte>(data.Length + headerSize);
        int encoded = LZ4Codec.Encode(data, compressedData.AsSpan(headerSize), level);

        // Write Compressed and Uncompressed.
        using var memStream = new MemoryStream(compressedData);
        memStream.Write<int>(encoded);
        memStream.Write<int>(data.Length);
        return compressedData.AsSpan(0, encoded + headerSize);
    }

    /// <summary>
    /// Writes a DDS.LZ4 to a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="data">The data to compress.</param>
    /// <param name="level">The compression level.</param>
    public static void PickleToStream(Stream stream, Span<byte> data, LZ4Level level = LZ4Level.L12_MAX)
    {
        var result = PickleToArray(data, level);
        stream.Write(result);
    }

    /// <summary>
    /// Writes a DDS.LZ4 to a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="data">The data to compress.</param>
    /// <param name="level">The compression level.</param>
    public static void PickleToFile(string filePath, Span<byte> data, LZ4Level level = LZ4Level.L12_MAX)
    {
        var result = PickleToArray(data, level);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        fileStream.Write(result);
    }
}
