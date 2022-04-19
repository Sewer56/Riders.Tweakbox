using System;
using System.Collections.Generic;
using Reloaded.Mod.Interfaces;
using Riders.Tweakbox.Services.Common;
using Riders.Tweakbox.Services.Interfaces;

namespace Riders.Tweakbox.Services.ObjectLayout;

/// <summary>
/// Keeps track of all stage layouts provided by other mods.
/// </summary>
public class ObjectLayoutService : FileService<ObjectLayoutDictionary>, ISingletonService
{
    public ObjectLayoutService(IModLoader modLoader) : base(modLoader, @"/Tweakbox/ObjectLayouts") { }

    /// <summary>
    /// Gets the file path to an alternative track layout for a given file name.
    /// </summary>
    /// <param name="fileName">The file name for which to get a replacement layout.</param>
    /// <param name="allowVanillaFile">Set to true if the vanilla file should be allowed to be used.</param>
    /// <returns>Path to the replacement layout. Null if no replacement exists or should use vanilla file.</returns>
    public unsafe string GetRandomLayout(string fileName, bool allowVanillaFile = true)
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
    /// Obtains all potential candidate alternative layouts for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetLayoutsForStage(int stageId, List<string> files) => GetFilesForFileName(GetFileNameForStageId(stageId), files);

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
    public string GetFileNameForStageId(int stageId) => $"STG{stageId:00}.layout";
}