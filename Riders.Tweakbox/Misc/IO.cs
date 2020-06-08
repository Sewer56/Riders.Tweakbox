using System;
using System.IO;
using System.Text.Json;
using K4os.Compression.LZ4;
using Riders.Tweakbox.Components.Netplay;
using static Riders.Tweakbox.Misc.FileSystemWatcherFactory;

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
        public string GearConfigFolder => Path.Combine(ConfigFolder, "GearConfigurations");
        public string NetplayConfigFolder => Path.Combine(ConfigFolder, "NetplayConfig");
        public string PhysicsConfigFolder => Path.Combine(ConfigFolder, "PhysicsConfigurations");
        public string NetplayConfigPath => Path.Combine(NetplayConfigFolder, "Config.json");

        /// <summary>
        /// Folder mod is stored in.
        /// </summary>
        public string ModFolder;

        public IO (string modFolder)
        {
            ModFolder = modFolder;
            Directory.CreateDirectory(TweakboxConfigFolder);
            Directory.CreateDirectory(GearConfigFolder);
            Directory.CreateDirectory(PhysicsConfigFolder);
            Directory.CreateDirectory(NetplayConfigFolder);
        }

        public string[] GetTweakboxConfigFiles() => Directory.GetFiles(TweakboxConfigFolder, ConfigSearchPattern);
        public string[] GetGearConfigFiles() => Directory.GetFiles(GearConfigFolder, ConfigSearchPattern);
        public string[] GetPhysicsConfigFiles() => Directory.GetFiles(PhysicsConfigFolder, ConfigSearchPattern);

        public NetplayConfig GetNetplayConfig() => File.Exists(NetplayConfigPath) ? JsonSerializer.Deserialize<NetplayConfig>(NetplayConfigPath) : new NetplayConfig();
        public void SaveNetplayConfig(NetplayConfig config) => File.WriteAllText(NetplayConfigPath, JsonSerializer.Serialize<NetplayConfig>(config));

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
        /// Creates a <see cref="FileSystemWatcher"/> which calls <param name="action"> when any of the events occur to the file.
        /// </summary>
        /// <param name="configDirectory">The path to monitor.</param>
        /// <param name="action">The function to run.</param>
        /// <param name="events">The events which trigger the action.</param>
        public static FileSystemWatcher CreateConfigWatcher(string configDirectory, Action action, FileSystemWatcherEvents events) => FileSystemWatcherFactory.CreateGeneric(configDirectory, action, events);
    }
}
