using System;
using System.Collections.Generic;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Services.Common;

/// <summary>
/// Provides the base code for a service which performs monitoring of files.
/// </summary>
public class FileService<T> : IFileService where T : FileDictionary, new()
{
    /// <summary>
    /// The dictionaries responsible for monitoring the files.
    /// </summary>
    public List<T> Dictionaries { get; private set; } = new List<T>();

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    public IModLoader ModLoader { get; private set; }

    /// <summary>
    /// Path to the folder (relative to mod folder) storing files to be monitored.
    /// </summary>
    public string RedirectFolderPath { get; private set; }

    /// <summary>
    /// The seed with which the random component was last initialised.
    /// </summary>
    public int Seed { get; private set; }

    protected Random Random { get; private set; }
    private Logger _log = new Logger(LogCategory.Random);

    public FileService(IModLoader modLoader, string redirectPath)
    {
        Random = new Random(Seed);
        RedirectFolderPath = redirectPath;
        ModLoader = modLoader;
        ModLoader.ModLoading += OnModLoading;
        ModLoader.ModUnloading += OnModUnloading;

        // Fill in textures from already loaded mods.
        var existingMods = ModLoader.GetActiveMods().Select(x => x.Generic);
        foreach (var mod in existingMods)
            Add(mod);

        IFileService.Services.Add(this);
    }

    /// <summary>
    /// Seeds the random number generator tied to this service.
    /// </summary>
    public void SeedRandom(int seed)
    {
        Seed = seed;
        Random = new Random(seed);
        _log.WriteLine($"[{nameof(FileService<T>)}] Seeding with {seed}");
    }

    /// <summary>
    /// Obtains all potential candidate files for a given file name.
    /// </summary>
    /// <param name="fileName">The name of the file the alternatives for.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetFilesForFileName(string fileName, List<string> files)
    {
        foreach (var dictionary in Dictionaries)
        {
            if (dictionary.TryGetValue(fileName, out var modTracks))
                files.AddRange(modTracks);
        }
    }

    private void Add(IModConfigV1 config)
    {
        var items = new T();
        items.Initialize(GetRedirectFolder(config.ModId));
        Dictionaries.Add(items);
    }

    private void Remove(IModConfigV1 config)
    {
        var redirectFolder = GetRedirectFolder(config.ModId);
        Dictionaries = Dictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
    private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);

    private string GetRedirectFolder(string modId) => ModLoader.GetDirectoryForModId(modId) + RedirectFolderPath;
}

public interface IFileService
{
    /// <summary>
    /// List of all active file services.
    /// </summary>
    public static List<IFileService> Services { get; private set; } = new List<IFileService>();

    /// <summary>
    /// Seeds all available services.
    /// </summary>
    public static void SeedAll(int seed)
    {
        foreach (var service in Services)
            service.SeedRandom(seed);
    }

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    IModLoader ModLoader { get; }

    /// <summary>
    /// Path to the folder (relative to mod folder) storing files to be monitored.
    /// </summary>
    string RedirectFolderPath { get; }

    /// <summary>
    /// The seed with which the random component was last initialised.
    /// </summary>
    int Seed { get; }

    /// <summary>
    /// Seeds the random number generator tied to this service.
    /// </summary>
    void SeedRandom(int seed);

    /// <summary>
    /// Obtains all potential candidate files for a given file name.
    /// </summary>
    /// <param name="fileName">The name of the file the alternatives for.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    void GetFilesForFileName(string fileName, List<string> files);


}