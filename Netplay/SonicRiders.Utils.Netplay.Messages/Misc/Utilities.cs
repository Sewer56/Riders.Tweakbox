﻿using System;
using System.IO;
using EnumsNET;
using MessagePack;
using Reloaded.Memory.Streams;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Misc;

public static class Utilities
{
    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static void ReadIfHasFlags<TType, TEnum>(this BufferedStreamReader reader, ref TType? value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum
    {
        if (flags.HasAllFlags(flagsToCheck))
        {
            reader.Read(out TType result);
            value = result;
        }
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static void ReadIfHasFlags<TStreamType, TType, TEnum>(this ref BitStream<TStreamType> reader, ref TType value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum where TStreamType : IByteStream
    {
        if (flags.HasAllFlags(flagsToCheck))
        {
            reader.ReadGeneric(out TType result);
            value = result;
        }
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static void ReadIfHasFlags<TStreamType, TType, TEnum>(this ref BitStream<TStreamType> reader, ref TType[] value, int numElements, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum where TStreamType : IByteStream
    {
        if (flags.HasAllFlags(flagsToCheck))
        {
            value = new TType[numElements];
            for (int x = 0; x < numElements; x++)
                reader.ReadGeneric(out value[x]);
        }
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static void ReadIfHasFlags<TType, TEnum>(this BufferedStreamReader reader, ref TType value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum
    {
        if (flags.HasAllFlags(flagsToCheck))
        {
            reader.Read(out TType result);
            value = result;
        }
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static void ReadIfHasFlags<TType, TEnum>(this BufferedStreamReader reader, ref TType[] value, int numElements, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum
    {
        if (flags.HasAllFlags(flagsToCheck))
        {
            value = new TType[numElements];
            for (int x = 0; x < numElements; x++)
                reader.Read(out value[x]);
        }
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static unsafe void ReadStructIfHasFlags<TStreamContainer, TType, TEnum>(ref this BitStream<TStreamContainer> reader, ref TType? value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum where TStreamContainer : IByteStream
    {
        if (flags.HasAllFlags(flagsToCheck))
            value = reader.ReadGeneric<TType>();
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static unsafe void ReadStructIfHasFlags<TStreamContainer, TType, TEnum>(ref this BitStream<TStreamContainer> reader, ref TType? value, TEnum flags, TEnum flagsToCheck, int numBits) where TType : unmanaged where TEnum : struct, Enum where TStreamContainer : IByteStream
    {
        if (flags.HasAllFlags(flagsToCheck))
            value = reader.ReadGeneric<TType>(numBits);
    }

    /// <summary>
    /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
    /// </summary>
    public static unsafe void ReadIfHasFlags<TStreamContainer, TType, TEnum>(ref this BitStream<TStreamContainer> reader, ref TType? value, TEnum flags, TEnum flagsToCheck, int numBits) where TType : unmanaged where TEnum : struct, Enum where TStreamContainer : IByteStream
    {
        if (flags.HasAllFlags(flagsToCheck))
            value = reader.Read<TType>(numBits);
    }

    /// <summary>
    /// Writes a generic span to a given BitStream.
    /// </summary>
    public static void WriteGeneric<T, TStreamType>(ref this BitStream<TStreamType> bitStream, Span<T> value) where T : unmanaged where TStreamType : IByteStream
    {
        for (int x = 0; x < value.Length; x++)
            bitStream.WriteGeneric(value[x]);
    }

    /// <summary>
    /// Deserializes a MessagePack packed message from a given stream.
    /// </summary>
    public static unsafe T DeserializeMessagePack<T>(Span<byte> bytes, out int numBytesRead, MessagePackSerializerOptions options = null)
    {
        fixed (byte* bytePtr = &bytes[0])
        {
            using Stream stream = new UnmanagedMemoryStream(bytePtr, bytes.Length);
            return DeserializeMessagePack<T>(stream, out numBytesRead, options);
        }
    }

    /// <summary>
    /// Deserializes a MessagePack packed message from a given stream.
    /// </summary>
    public static unsafe T DeserializeMessagePack<T>(Stream stream, out int numBytesRead, MessagePackSerializerOptions options = null)
    {
        var originalPosition = stream.Position;
        var value = MessagePackSerializer.Deserialize<T>(stream, options);
        numBytesRead = (int)(stream.Position - originalPosition);

        return value;
    }

    /// <summary>
    /// Deserialized a MessagePack packed message from a given stream.
    /// </summary>
    public static T DeserializeMessagePack<T>(BufferedStreamReader reader, MessagePackSerializerOptions options = null)
    {
        var baseStream = reader.BaseStream();
        baseStream.Position = reader.Position();

        var value = MessagePackSerializer.Deserialize<T>(baseStream, options);
        reader.Seek(baseStream.Position, SeekOrigin.Begin);

        return value;
    }

    /// <summary>
    /// Rounds up a specified number to the next multiple of X.
    /// </summary>
    /// <param name="number">The number to round up.</param>
    /// <param name="multiple">The multiple the number should be rounded to.</param>
    /// <returns></returns>
    public static long RoundUp(long number, long multiple)
    {
        if (multiple == 0)
            return number;

        long remainder = number % multiple;
        if (remainder == 0)
            return number;

        return number + multiple - remainder;
    }

    /// <summary>
    /// Rounds a number down to the previous multiple unless it is already a multiple.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <param name="multiple">The multiple.</param>
    public static int RoundDown(int number, int multiple)
    {
        if (multiple == 0)
            return number;

        return number / multiple * multiple;
    }

    /// <summary>
    /// Gets the minimum number of bits necessary to represent this number.
    /// </summary>
    /// <param name="value">The number.</param>
    public static int GetMinimumNumberOfBits(int value)
    {
        int bits = 0;
        while ((value >>= 1) != 0)
            bits++;

        return bits + 1;
    }
}
