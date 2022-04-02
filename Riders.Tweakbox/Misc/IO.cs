using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

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

    private Dictionary<string, string> _licenseNameToTextMap = new Dictionary<string, string>();

    // Configuration Directories.
    public string ConfigFolder => Path.Combine(ReloadedConfigFolder, "Configurations");
    public string ConfigFolderOld => Path.Combine(ModFolder, "Configurations"); // Obsolete

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
    public string ExportFolder => Path.Combine(ConfigFolder, "Export");
    public string TextureDumpCommonFolder => Path.Combine(TextureDumpFolder, "Auto Common");
    public string TextureCacheFilePath => Path.Combine(TextureCacheFolder, "TextureCache.msgpack");
    public string FirstTimeFlagPath => Path.Combine(ConfigFolder, "FirstTime.txt");
    public string AssetsFolder => Path.Combine(ModFolder, "Assets");
    public string LicenseFolder => Path.Combine(AssetsFolder, "License");

    /// <summary>
    /// Folder mod is stored in.
    /// </summary>
    public string ModFolder;

    /// <summary>
    /// Folder in which Reloaded stores its configurations.
    /// </summary>
    public string ReloadedConfigFolder;

    public IO(string modFolder, string configFolder)
    {
        ModFolder = modFolder;
        ReloadedConfigFolder = configFolder;

        try { MoveDirectory(ConfigFolderOld, ConfigFolder); }
        catch (Exception) { /* Ignored */ }

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
        Directory.CreateDirectory(ExportFolder);
    }

    public string[] GetGearConfigFiles() => Directory.GetFiles(GearConfigFolder, ConfigSearchPattern);
    public string[] GetPhysicsConfigFiles() => Directory.GetFiles(PhysicsConfigFolder, ConfigSearchPattern);
    public string[] GetFixesConfigFiles() => Directory.GetFiles(FixesConfigFolder, JsonSearchPattern);
    public string[] GetTextureConfigFiles() => Directory.GetFiles(TextureConfigFolder, JsonSearchPattern);
    public string[] GetLogsConfigFiles() => Directory.GetFiles(LogConfigFolder, JsonSearchPattern);
    public string[] GetNetplayConfigFiles() => Directory.GetFiles(NetplayConfigFolder, JsonSearchPattern);
    public string[] GetInfoConfigFiles() => Directory.GetFiles(InfoConfigFolder, ConfigSearchPattern);

    /// <summary>
    /// Gets the license given a specific file name.
    /// </summary>
    /// <param name="fileNameWithoutExtension">File name without extension.</param>
    public string GetLicenseFile(string fileNameWithoutExtension)
    {
        var filePath = Path.Combine(LicenseFolder, $"{fileNameWithoutExtension}.txt");
        if (_licenseNameToTextMap.TryGetValue(fileNameWithoutExtension, out var text))
            return text;
        
        if (File.Exists(filePath))
        {
            text = File.ReadAllText(filePath);
            _licenseNameToTextMap[filePath] = text;
            return text;
        }

        return "";
    }

    /// <summary>
    /// Moves a directory from a given source path to a target path, overwriting all files.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="target">The target path.</param>
    public static void MoveDirectory(string source, string target)
    {
        MoveDirectory(source, target, (x, y) =>
        {
            File.Copy(x, y, true);
            File.Delete(x);
        });
    }
    
    private static void MoveDirectory(string source, string target, Action<string, string> moveDirectoryAction)
    {
        Directory.CreateDirectory(target);

        // Get all files in source directory.
        var sourceFilePaths = Directory.EnumerateFiles(source);

        // Move them.
        foreach (var sourceFilePath in sourceFilePaths)
        {
            // Get destination file path
            var destFileName = Path.GetFileName(sourceFilePath);
            var destFilePath = Path.Combine(target, destFileName);

            while (File.Exists(destFilePath) && !CheckFileAccess(destFilePath, FileMode.Open, FileAccess.Write))
                Thread.Sleep(100);

            if (File.Exists(destFilePath))
                File.Delete(destFilePath);

            moveDirectoryAction(sourceFilePath, destFilePath);
        }

        // Get all subdirectories in source directory.
        var sourceSubDirPaths = Directory.EnumerateDirectories(source);

        // Recursively move them.
        foreach (var sourceSubDirPath in sourceSubDirPaths)
        {
            var destSubDirName = Path.GetFileName(sourceSubDirPath);
            var destSubDirPath = Path.Combine(target, destSubDirName);
            MoveDirectory(sourceSubDirPath, destSubDirPath, moveDirectoryAction);
        }
    }

    /// <summary>
    /// Tries to open a stream for a specified file.
    /// Returns null if it fails due to file lock.
    /// </summary>
    public static FileStream TryOpenOrCreateFileStream(string filePath, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite)
    {
        try
        {
            return File.Open(filePath, mode, access);
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// Checks whether a file with a specific path can be opened.
    /// </summary>
    public static bool CheckFileAccess(string filePath, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite)
    {
        using var stream = TryOpenOrCreateFileStream(filePath, mode, access);
        return stream != null;
    }
}
