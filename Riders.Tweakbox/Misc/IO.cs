using System.Diagnostics;
using System.IO;
namespace Riders.Tweakbox.Misc;

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
    public string InfoConfigFolder => Path.Combine(ConfigFolder, "InfoConfigurations");
    public string TextureCacheFolder => Path.Combine(ConfigFolder, "TextureCache");
    public string TextureCacheFilesFolder => Path.Combine(TextureCacheFolder, "Cache");
    public string TextureDumpFolder => Path.Combine(ConfigFolder, "TextureDumps");
    public string AutosaveFolder => Path.Combine(ConfigFolder, "Autosave");
    public string AutosaveObjectLayoutFolder => Path.Combine(AutosaveFolder, "ObjectLayouts");
    public string TextureDumpCommonFolder => Path.Combine(TextureDumpFolder, "Auto Common");
    public string TextureCacheFilePath => Path.Combine(TextureCacheFolder, "TextureCache.msgpack");
    public string FirstTimeFlagPath => Path.Combine(ConfigFolder, "FirstTime.txt");

    public string AssetsFolder => Path.Combine(ModFolder, "Assets");

    /// <summary>
    /// Folder mod is stored in.
    /// </summary>
    public string ModFolder;

    public IO(string modFolder)
    {
        ModFolder = modFolder;
        Directory.CreateDirectory(FixesConfigFolder);
        Directory.CreateDirectory(GearConfigFolder);
        Directory.CreateDirectory(PhysicsConfigFolder);
        Directory.CreateDirectory(NetplayConfigFolder);
        Directory.CreateDirectory(LogConfigFolder);
        Directory.CreateDirectory(TextureCacheFilesFolder);
        Directory.CreateDirectory(TextureDumpFolder);
        Directory.CreateDirectory(TextureDumpCommonFolder);
        Directory.CreateDirectory(TextureConfigFolder);
        Directory.CreateDirectory(InfoConfigFolder);
        Directory.CreateDirectory(AutosaveFolder);
        Directory.CreateDirectory(AutosaveObjectLayoutFolder);
    }

    public string[] GetGearConfigFiles() => Directory.GetFiles(GearConfigFolder, ConfigSearchPattern);
    public string[] GetPhysicsConfigFiles() => Directory.GetFiles(PhysicsConfigFolder, ConfigSearchPattern);
    public string[] GetFixesConfigFiles() => Directory.GetFiles(FixesConfigFolder, JsonSearchPattern);
    public string[] GetTextureConfigFiles() => Directory.GetFiles(TextureConfigFolder, JsonSearchPattern);
    public string[] GetLogsConfigFiles() => Directory.GetFiles(LogConfigFolder, JsonSearchPattern);
    public string[] GetNetplayConfigFiles() => Directory.GetFiles(NetplayConfigFolder, JsonSearchPattern);
    public string[] GetInfoConfigFiles() => Directory.GetFiles(InfoConfigFolder, ConfigSearchPattern);
}
