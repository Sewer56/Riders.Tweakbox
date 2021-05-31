using System.Diagnostics;
using System.IO;

namespace Riders.Tweakbox.Misc
{
    public class IO
    {
        // Extension used to store config files and pattern to find them.
        public const string JsonConfigExtension = ".json";
        public const string JsonSearchPattern = "*.json";

        public const string ConfigExtension = ".tweakbox";
        public const string ConfigSearchPattern = "*.tweakbox";
        public static string GameFolderLocation = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string DataFolderLocation = Path.Combine(GameFolderLocation, "Data");

        // Configuration Directories.
        public string ConfigFolder => Path.Combine(ModFolder, "Configurations");

        public string FixesConfigFolder => Path.Combine(ConfigFolder, "FixesConfigurations");
        public string GearConfigFolder => Path.Combine(ConfigFolder, "GearConfigurations");
        public string NetplayConfigFolder => Path.Combine(ConfigFolder, "NetplayConfig");
        public string PhysicsConfigFolder => Path.Combine(ConfigFolder, "PhysicsConfigurations");
        public string LogConfigFolder => Path.Combine(ConfigFolder, "LogConfigurations");
        public string TextureConfigFolder => Path.Combine(ConfigFolder, "TextureConfigurations");
        public string TextureDumpFolder => Path.Combine(ConfigFolder, "TextureDumps");
        public string TextureDumpCommonFolder => Path.Combine(TextureDumpFolder, "Auto Common");
        public string FirstTimeFlagPath => Path.Combine(ConfigFolder, "FirstTime.txt");

        /// <summary>
        /// Folder mod is stored in.
        /// </summary>
        public string ModFolder;

        public IO (string modFolder)
        {
            ModFolder = modFolder;
            Directory.CreateDirectory(FixesConfigFolder);
            Directory.CreateDirectory(GearConfigFolder);
            Directory.CreateDirectory(PhysicsConfigFolder);
            Directory.CreateDirectory(NetplayConfigFolder);
            Directory.CreateDirectory(LogConfigFolder);
            Directory.CreateDirectory(TextureDumpFolder);
            Directory.CreateDirectory(TextureDumpCommonFolder);
            Directory.CreateDirectory(TextureConfigFolder);
        }

        public string[] GetGearConfigFiles() => Directory.GetFiles(GearConfigFolder, ConfigSearchPattern);
        public string[] GetPhysicsConfigFiles() => Directory.GetFiles(PhysicsConfigFolder, ConfigSearchPattern);
        public string[] GetFixesConfigFiles() => Directory.GetFiles(FixesConfigFolder, JsonSearchPattern);
        public string[] GetTextureConfigFiles() => Directory.GetFiles(TextureConfigFolder, JsonSearchPattern);
        public string[] GetLogsConfigFiles() => Directory.GetFiles(LogConfigFolder, JsonSearchPattern);
        public string[] GetNetplayConfigFiles() => Directory.GetFiles(NetplayConfigFolder, JsonSearchPattern);
    }
}
