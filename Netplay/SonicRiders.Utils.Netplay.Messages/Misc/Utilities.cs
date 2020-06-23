using System;
using System.IO;
using BitStreams;
using EnumsNET;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using MessagePack;
using Reloaded.Memory;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages.Misc
{
    public static class Utilities
    {
        /// <summary>
        /// Reads a given type from the bitstream.
        /// </summary>
        public static TType Read<TType>(this BitStream reader) where TType : unmanaged
        {
            var size = Struct.GetSize<TType>();
            var bytes = reader.ReadBytes(size, true);
            Struct.FromArray(bytes, out TType val, 0);
            return val;
        }

        /// <summary>
        /// Reads a given type from the bitstream with a specified number of bits.
        /// </summary>
        public static unsafe TType Read<TType>(this BitStream reader, int numBits) where TType : unmanaged
        {
            var output  = stackalloc byte[sizeof(TType)];
            var bytes   = reader.ReadBytes(numBits, false);

            var outputSpan  = new Span<byte>(output, sizeof(TType));
            var bytesSpan   = new Span<byte>(bytes);
            bytesSpan.CopyTo(outputSpan);
            Struct.FromArray(outputSpan, out TType val);

            return val;
        }

        /// <summary>
        /// Writes a struct value to a <see cref="BitStream"/>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value to append to the stream.</param>
        public static void Write<T>(this BitStream stream, T value) where T : unmanaged
        {
            var data = Struct.GetBytes(value);
            stream.WriteBytes(data, data.Length, true);
        }

        /// <summary>
        /// Writes a struct value to a <see cref="BitStream"/>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value to append to the stream.</param>
        /// <param name="length">Number of bits to reserve for the structure.</param>
        public static void Write<T>(this BitStream stream, T value, int length) where T : unmanaged
        {
            var data = Struct.GetBytes(value);
            stream.WriteBytes(data, length, false);
        }

        /// <summary>
        /// Writes a <see cref="Nullable"/> value to a <see cref="BitStream"/>.
        /// If a value exists, it is written, else nothing is done.
        /// </summary>
        public static void WriteNullable<T>(this BitStream stream, T? nullable) where T : unmanaged
        {
            if (nullable.HasValue)
                Write(stream, nullable.Value);
        }

        /// <summary>
        /// Writes a <see cref="Nullable"/> value to a <see cref="ExtendedMemoryStream"/>.
        /// If a value exists, it is written, else nothing is done.
        /// </summary>
        public static void WriteNullable<T>(this ExtendedMemoryStream stream, T? nullable) where T : unmanaged
        {
            if (nullable.HasValue)
                stream.Write(nullable.Value);
        }

        /// <summary>
        /// Writes a <see cref="Nullable"/> value to a <see cref="BitStream"/>.
        /// If a value exists, it is written, else nothing is done.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="nullable">The value to append to the stream.</param>
        /// <param name="length">Number of bits to reserve for the structure.</param>
        public static void WriteNullable<T>(this BitStream stream, T? nullable, int length) where T : unmanaged
        {
            if (nullable.HasValue)
            {
                var data = Struct.GetBytes(nullable.Value);
                stream.WriteBytes(data, length, false);
            }
        }

        /// <summary>
        /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
        /// </summary>
        public static void ReadIfHasFlags<TType, TEnum>(this BitStream reader, ref TType? value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum
        {
            if (flags.HasAllFlags(flagsToCheck))
                value = Read<TType>(reader);
        }

        /// <summary>
        /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
        /// </summary>
        public static unsafe void ReadIfHasFlags<TType, TEnum>(this BitStream reader, ref TType? value, TEnum flags, TEnum flagsToCheck, int numBits) where TType : unmanaged where TEnum : struct, Enum
        {
            if (flags.HasAllFlags(flagsToCheck))
                value = Read<TType>(reader, numBits);
        }

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
        /// Executes a function <see cref="Action"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
        /// </summary>
        public static void ExecuteIfHasFlags<TEnum>(this TEnum flags, TEnum flagsToCheck, Action action) where TEnum : struct, Enum
        {
            if (flags.HasAllFlags(flagsToCheck))
                action();
        }

        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        public static byte[] CompressLZ4(byte[] source, LZ4Level level = LZ4Level.L10_OPT)
        {
            var target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            var encodedLength = LZ4Codec.Encode(source, 0, source.Length, target, 0, target.Length);
            return new Span<byte>(target).Slice(0, encodedLength).ToArray();
        }

        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        public static byte[] CompressLZ4Stream(byte[] source, LZ4Level level = LZ4Level.L10_OPT)
        {
            var target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            
            using (var targetStream = new MemoryStream(target, true))
            using (var sourceStream = new MemoryStream(source, true))
            using (var compStream = LZ4Stream.Encode(targetStream, level, 0, true))
            {
                sourceStream.CopyTo(compStream);
                compStream.Close();
                return new Span<byte>(target).Slice(0, (int) targetStream.Position).ToArray();
            }
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to decompress.</param>
        public static byte[] DecompressLZ4(byte[] source)
        {
            // byte.MaxValue = Maximum possible output size per byte.
            var target = new byte[source.Length * byte.MaxValue];
            var decodedLength = LZ4Codec.Decode(source, 0, source.Length, target, 0, target.Length);
            return new Span<byte>(target).Slice(0, decodedLength).ToArray();
        }

        /// <summary>
        /// Decompresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="buffer">Preallocated buffer of expected size for data.</param>
        /// <param name="reader">Reader to decompress data from.</param>
        public static byte[] DecompressLZ4Stream(byte[] buffer, BufferedStreamReader reader)
        {
            // Move stream to read position / Reloaded.Memory does not sync base stream offset. 
            var baseStream      = reader.BaseStream();
            baseStream.Position = reader.Position();

            using (var targetStream = new MemoryStream(buffer, true))
            using (var decompStream = LZ4Stream.Decode(baseStream, 0, true))
            {
                decompStream.CopyTo(targetStream);

                // Seek Reloaded.Memory stream to new address.
                reader.Seek(decompStream.Position, SeekOrigin.Begin);
            }

            return buffer;
        }

        /// <summary>
        /// Deserialized a MessagePack packed message from a given stream.
        /// </summary>
        public static T DesrializeMessagePack<T>(BufferedStreamReader reader)
        {
            var baseStream = reader.BaseStream();
            baseStream.Position = reader.Position();

            var value = MessagePackSerializer.Deserialize<T>(baseStream);
            reader.Seek(baseStream.Position, SeekOrigin.Begin);

            return value;
        }

    }
}
