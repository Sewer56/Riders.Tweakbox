using System;
using System.Collections.Generic;
using Reloaded.Mod.Interfaces;
using Riders.Tweakbox.Services.Common;
using Riders.Tweakbox.Services.Interfaces;

namespace Riders.Tweakbox.Services.Rails;

/// <summary>
/// Keeps track of all stage layouts provided by other mods.
/// </summary>
public class RailService : FileService<RailDictionary>, ISingletonService
{
    public RailService(IModLoader modLoader) : base(modLoader, @"/Tweakbox/Rails") { }

    /// <summary>
    /// Gets the file path to an alternative track layout for a given file name.
    /// </summary>
    /// <param name="fileName">The file name for which to get a replacement layout.</param>
    /// <param name="allowVanillaFile">Set to true if the vanilla file should be allowed to be used.</param>
    /// <returns>Path to the replacement layout. Null if no replacement exists or should use vanilla file.</returns>
    public unsafe string GetRandomRails(string fileName, bool allowVanillaFile = true)
    {
        var options = new List<string>();
        GetFilesForFileName(fileName, options);
        if (options.Count <= 0)
            return null;

        var random = Random.Next(0, options.Count + Convert.ToInt32(allowVanillaFile));

        // If last item, assume vanilla layout.
        if (allowVanillaFile && random == options.Count)
            return null;

        return options[random];
    }

    /// <summary>
    /// Obtains all potential candidate alternative rail configurations for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetRailsForStage(int stageId, List<string> files) => GetFilesForFileName(GetFileNameForStageId(stageId), files);

    /// <summary>
    /// Obtains a random alternative rail configuration for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="allowVanillaFile">Set to true if the vanilla file should be allowed to be used.</param>
    /// <returns>Path to the replacement rails. Null if no replacement exists or should use vanilla file.</returns>
    public string GetRandomRailsForStage(int stageId, bool allowVanillaFile = true) => GetRandomRails(GetFileNameForStageId(stageId), allowVanillaFile);

    /// <summary>
    /// Obtains the file name for a given stage id.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    public string GetFileNameForStageId(int stageId) => $"STG{stageId:00}.json";
}