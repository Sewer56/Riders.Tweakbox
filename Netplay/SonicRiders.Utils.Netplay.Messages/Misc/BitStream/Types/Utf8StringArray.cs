using System.Runtime.CompilerServices;
using System.Text;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.BitStream.Types;

/// <summary>
/// Array of strings encoded using UTF8
/// </summary>
public struct Utf8StringArray
{
    private const int MaxStringSize = 4096;
    private static readonly Encoding _encoding        = Encoding.UTF8;
    private static readonly int _nullTerminatorLength = _encoding.GetByteCount("\0");

    /// <summary>
    /// The data to be serialized/deserialized.
    /// </summary>
    public string[] Data { get; set; }

    public Utf8StringArray(string[] array) => Data = array;

    /// <summary>
    /// Retrieves the approximate size of data (in bytes) to be sent over the network.
    /// </summary>
    /// <param name="arraySizeBits">Number of bits used for encoding the array length.</param>
    public unsafe int GetDataSize(int arraySizeBits)
    {
        // Array size.
        var arraySizeBytes = (arraySizeBits / 8) + 1;
        foreach (var str in Data)
            arraySizeBytes += _encoding.GetByteCount(str) + _nullTerminatorLength;
        
        return arraySizeBytes;
    }

    [SkipLocalsInit]
    public static Utf8StringArray Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, int arraySizeBits) where TByteStream : IByteStream
    {
        var thisObject = new Utf8StringArray();
        thisObject.FromStream(ref bitStream, arraySizeBits);
        return thisObject;
    }

    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream, int arraySizeBits) where TByteStream : IByteStream
    {
        Data = new string[bitStream.Read<int>(arraySizeBits)];
        for (int x = 0; x < Data.Length; x++)
            Data[x] = bitStream.ReadString(MaxStringSize, _encoding);
    }
    
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream, int arraySizeBits) where TByteStream : IByteStream
    {
        bitStream.Write(Data.Length, arraySizeBits);
        foreach (var str in Data)
            bitStream.WriteString(str, MaxStringSize, _encoding);
    }

    // Conversions
    public static implicit operator string[](Utf8StringArray array) => array.Data;
    public static implicit operator Utf8StringArray(string[] array) => new(array);
}