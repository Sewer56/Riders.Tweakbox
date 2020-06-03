using System;
using System.IO;
using EnumsNET;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages
{
    public static class Utilities
    {
        /// <summary>
        /// Writes a <see cref="Nullable"/> value to a <see cref="ExtendedMemoryStream"/>.
        /// If a value exists, it is written, else nothing is done.
        /// </summary>
        public static void WriteNullable<T>(this ExtendedMemoryStream stream, T? nullable, bool marshalStructure = false) where T : struct
        {
            if (nullable.HasValue)
                stream.Write(nullable.Value, marshalStructure);
        }

        /// <summary>
        /// Sets a given parameter marked by <see cref="value"/> if <see cref="flags"/> contains a flag <see cref="flagsToCheck"/>.
        /// </summary>
        public static void SetValueIfHasFlags<TType, TEnum>(this BufferedStreamReader reader, ref TType? value, TEnum flags, TEnum flagsToCheck) where TType : unmanaged where TEnum : struct, Enum
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
    }
}
