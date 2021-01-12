using System;
using System.IO;
using K4os.Compression.LZ4;

namespace Riders.Tweakbox.Misc
{
    public class IO
    {
        // Extension used to store config files and pattern to find them.
        public static readonly string ConfigExtension = ".tweakbox";
        public static readonly string ConfigSearchPattern = $"*.tweakbox";

        // Configuration Directories.
        public string ConfigFolder => Path.Combine(ModFolder, "Configurations");
        public string TweakboxConfigFolder => Path.Combine(ConfigFolder, "TweakboxConfigurations");

        public string FixesConfigFolder => Path.Combine(ConfigFolder, "FixesConfigurations");
        public string GearConfigFolder => Path.Combine(ConfigFolder, "GearConfigurations");
        public string NetplayConfigFolder => Path.Combine(ConfigFolder, "NetplayConfig");
        public string PhysicsConfigFolder => Path.Combine(ConfigFolder, "PhysicsConfigurations");
        public string LogConfigFolder => Path.Combine(ConfigFolder, "LogConfigurations");

        /// <summary>
        /// Folder mod is stored in.
        /// </summary>
        public string ModFolder;

        public IO (string modFolder)
        {
            ModFolder = modFolder;
            Directory.CreateDirectory(TweakboxConfigFolder);
            Directory.CreateDirectory(FixesConfigFolder);
            Directory.CreateDirectory(GearConfigFolder);
            Directory.CreateDirectory(PhysicsConfigFolder);
            Directory.CreateDirectory(NetplayConfigFolder);
            Directory.CreateDirectory(LogConfigFolder);
        }

        public string[] GetTweakboxConfigFiles() => Directory.GetFiles(TweakboxConfigFolder, ConfigSearchPattern);
        public string[] GetGearConfigFiles() => Directory.GetFiles(GearConfigFolder, ConfigSearchPattern);
        public string[] GetPhysicsConfigFiles() => Directory.GetFiles(PhysicsConfigFolder, ConfigSearchPattern);
        public string[] GetFixesConfigFiles() => Directory.GetFiles(FixesConfigFolder, ConfigSearchPattern);
        public string[] GetLogsConfigFiles() => Directory.GetFiles(LogConfigFolder, ConfigSearchPattern);
        public string[] GetNetplayConfigFiles() => Directory.GetFiles(NetplayConfigFolder, ConfigSearchPattern);

        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        public static byte[] CompressLZ4(byte[] source, LZ4Level level = LZ4Level.L10_OPT)
        {
            var target        = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            var encodedLength = LZ4Codec.Encode(source, 0, source.Length, target, 0, target.Length);
            return new Span<byte>(target).Slice(0, encodedLength).ToArray();
        }

        /// <summary>
        /// Compresses a chunk of data using the LZ4 compression algorithm.
        /// </summary>
        /// <param name="source">The data to compress.</param>
        /// <param name="level">The level to compress at.</param>
        public static Span<byte> CompressLZ4(Span<byte> source, LZ4Level level = LZ4Level.L10_OPT)
        {
            var target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            var encodedLength = LZ4Codec.Encode(source, target);
            return new Span<byte>(target).Slice(0, encodedLength);
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
        /// <param name="source">The data to decompress.</param>
        public static Span<byte> DecompressLZ4(Span<byte> source)
        {
            // byte.MaxValue = Maximum possible output size per byte.
            var target = new byte[source.Length * byte.MaxValue];
            var decodedLength = LZ4Codec.Decode(source, target);
            return new Span<byte>(target).Slice(0, decodedLength).ToArray();
        }
    }
}
