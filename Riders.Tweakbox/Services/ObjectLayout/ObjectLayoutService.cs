using System;
using System.Collections.Generic;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Services.Interfaces;

namespace Riders.Tweakbox.Services.ObjectLayout;

/// <summary>
/// Keeps track of all stage layouts provided by other mods.
/// </summary>
public class ObjectLayoutService : ISingletonService
{
    private List<ObjectLayoutDictionary> _dictionaries = new List<ObjectLayoutDictionary>();
    private IModLoader _modLoader;

    public ObjectLayoutService(IModLoader modLoader)
    {
        _modLoader = modLoader;
        _modLoader.ModLoading += OnModLoading;
        _modLoader.ModUnloading += OnModUnloading;

        // Fill in textures from already loaded mods.
        var existingMods = _modLoader.GetActiveMods().Select(x => x.Generic);
        foreach (var mod in existingMods)
        {
            Add(mod);
        }
    }

    /// <summary>
    /// Gets the file path to an alternative track layout for a given file name.
    /// </summary>
    /// <param name="fileName">The file name for which to get a replacement layout.</param>
    /// <param name="allowVanillaFile">Set to true if the vanilla file should be allowed to be used.</param>
    /// <returns>Path to the replacement layout. Null if no replacement exists or should use vanilla file.</returns>
    public unsafe string GetRandomLayout(string fileName, bool allowVanillaFile = true)
    {
        var options = new List<string>();
        GetLayoutsForFileName(fileName, options);
        if (options.Count <= 0)
            return null;

        var random = Misc.Extensions.SharedRandom.Instance.Next(0, options.Count + Convert.ToInt32(allowVanillaFile));
        
        // If last item, assume vanilla layout.
        if (allowVanillaFile && random == options.Count)
            return null;

        return options[random];
    }

    /// <summary>
    /// Obtains all potential candidate alternative layouts for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetLayoutsForStage(int stageId, List<string> files) => GetLayoutsForFileName(GetFileNameForStageId(stageId), files);

    /// <summary>
    /// Obtains a random alternative stage layout for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="allowVanillaFile">Set to true if the vanilla file should be allowed to be used.</param>
    /// <returns>Path to the replacement layout. Null if no replacement exists or should use vanilla file.</returns>
    public string GetRandomLayoutForStage(int stageId, bool allowVanillaFile = true) => GetRandomLayout(GetFileNameForStageId(stageId), allowVanillaFile);

    /// <summary>
    /// Obtains the file name for a given stage id.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    public string GetFileNameForStageId(int stageId) => $"{stageId:00}.layout";

    /// <summary>
    /// Obtains all potential candidate tracks for a given file name.
    /// </summary>
    /// <param name="fileName">The name of the file to get the tracks for</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetLayoutsForFileName(string fileName, List<string> files)
    {
        foreach (var dictionary in _dictionaries)
        {
            if (dictionary.TryGetValue(fileName, out var modTracks))
                files.AddRange(modTracks);
        }
    }

    private void Add(IModConfigV1 config) => _dictionaries.Add(new ObjectLayoutDictionary(GetRedirectFolder(config.ModId)));
    private void Remove(IModConfigV1 config)
    {
        var redirectFolder = GetRedirectFolder(config.ModId);
        _dictionaries = _dictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
    private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);
    private string GetRedirectFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/ObjectLayouts";
}