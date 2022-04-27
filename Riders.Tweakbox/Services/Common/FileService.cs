using System;
using System.Collections.Generic;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Interfaces;
using StructLinq;

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
    /// List of all available dictionaries by Mod ID.
    /// </summary>
    public Dictionary<string, T> ModIdToDictionary { get; private set; } = new Dictionary<string, T>();

    /// <summary>
    /// List of all available mod IDs by dictionary.
    /// </summary>
    public Dictionary<T, string> DictionaryToModId { get; private set; } = new Dictionary<T, string>();

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
    /// Clears current list of enabled dictionaries.
    /// They still can be accessed by Mod ID.
    /// </summary>
    public void DisableAll() => Dictionaries.Clear();

    /// <summary>
    /// Enables a list of dictionaries using Mod IDs.  
    /// Disables all other dictionaries.  
    /// </summary>
    /// <param name="modIds">List of mod IDs to load dictionary from.</param>
    public void SetEnabledFromModIds(IEnumerable<string> modIds)
    {
        DisableAll();
        foreach (var modId in modIds)
            if (ModIdToDictionary.TryGetValue(modId, out var dict))
                Dictionaries.Add(dict);
    }

    /// <summary>
    /// Gets a list of mod IDs for which dictionaries are enabled.
    /// </summary>
    /// <returns>List of mod IDs that are currently enabled.</returns>
    public List<string> GetEnabledModIds(bool nonEmptyOnly = true)
    {
        var result = new List<string>();
        foreach (var dict in Dictionaries)
        {
            // Remove if no elements available.
            if (nonEmptyOnly && !dict.Any())
                continue;

            if (DictionaryToModId.TryGetValue(dict, out var modId))
                result.Add(modId);
        }

        return result;
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
        var dictionary = new T();
        dictionary.Initialize(GetRedirectFolder(config.ModId));

        Dictionaries.Add(dictionary);
        ModIdToDictionary[config.ModId] = dictionary;
        DictionaryToModId[dictionary] = config.ModId;
    }

    private void Remove(IModConfigV1 config)
    {
        if (ModIdToDictionary.Remove(config.ModId, out var dictionary))
        {
            DictionaryToModId.Remove(dictionary);
            Dictionaries.Remove(dictionary);
        }
    }

    private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
    private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);

    private string GetRedirectFolder(string modId) => ModLoader.GetDirectoryForModId(modId) + RedirectFolderPath;
}