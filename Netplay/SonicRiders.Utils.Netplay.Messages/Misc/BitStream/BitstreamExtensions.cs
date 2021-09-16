using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.BitStream;

public static class BitstreamExtensions
{
    /// <summary>
    /// Reads a string array from a given stream.
    /// </summary>
    public static string[] ReadStringArray<T>(this ref BitStream<T> stream, int maxLengthBytes = 1024, Encoding encoding = null) where T : IByteStream
    {
        int numItems = stream.Read<ushort>();
        var result = new string[numItems];
        for (int x = 0; x < numItems; x++)
            result[x] = stream.ReadString(maxLengthBytes, encoding);

        return result;
    }

    /// <summary>
    /// Writes a string array to a given stream.
    /// </summary>
    public static void WriteStringArray<T>(this ref BitStream<T> stream, string[] array, int maxLengthBytes = 1024, Encoding encoding = null) where T : IByteStream
    {
        Debug.Assert(array.Length < ushort.MaxValue);

        stream.Write<ushort>((ushort)array.Length);
        for (int x = 0; x < array.Length; x++)
            stream.WriteString(array[x], maxLengthBytes, encoding);
    }
}
