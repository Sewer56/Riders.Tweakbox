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
    /// Writes a DDS.LZ4 to a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="data">The data to compress.</param>
    public static void PickleToStream(Stream stream, Span<byte> data)
    {
        var compressedData = GC.AllocateUninitializedArray<byte>(data.Length);
        var encoded = LZ4Codec.Encode(data, compressedData, LZ4Level.L12_MAX);
        
        // Write Compressed and Uncompressed.
        stream.Write<int>(encoded);
        stream.Write<int>(data.Length);
        stream.Write(compressedData.AsSpan().Slice(0, encoded));
    }

    /// <summary>
    /// Writes a DDS.LZ4 to a given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="data">The data to compress.</param>
    public static void PickleToFile(string filePath, Span<byte> data)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create);
        PickleToStream(fileStream, data);
    }
}
