using System;
using System.Collections.Generic;
using System.IO;
using Reloaded.Mod.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Common;
using Riders.Tweakbox.Services.Interfaces;
using Sewer56.SonicRiders.API;
namespace Riders.Tweakbox.Services.Music;

/// <summary>
/// Keeps track of all music tracks provided by other mods (as well as the vanilla game)
/// </summary>
public class MusicService : FileService<MusicDictionary>, ISingletonService, INetplaySyncedFileService
{
    public int Id { get; } = 0x4F424A4C; // MSIC

    private static HashSet<string> _vanillaStageTracks = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "S1.adx", "S2.adx", "S3.adx", "S4.adx", "S5.adx", "S6.adx", "S7.adx", "S8.adx", "SV1.adx" };
    
    private MusicDictionary _vanillaDict;
    private IModLoader _modLoader;

    public MusicService(IModLoader modLoader) : base(modLoader, @"/Tweakbox/Music")
    {
        _vanillaDict = new MusicDictionary(Path.Combine(IO.GameFolderLocation, "Data"));
    }

    /// <summary>
    /// Gets the name of a random alternative track for a given file name.
    /// </summary>
    /// <param name="fileName">The file name for which to get a replacement track.</param>
    /// <param name="includeVanilla">Whether to include vanilla tracks or not.</param>
    /// <param name="includePerStageTracks">Whether to include stage-specific tracks.</param>
    /// <returns>Path to the replacement track.</returns>
    public unsafe string GetRandomTrack(string fileName, bool includeVanilla, bool includePerStageTracks)
    {
        var options = new List<string>();

        if (includePerStageTracks && _vanillaStageTracks.Contains(fileName))
            GetTracksForStage(*(int*)State.Level, options);

        GetFilesForFileName(fileName, options);

        if ((options.Count < 1 || includeVanilla) && _vanillaDict.TryGetValue(fileName, out var vanillaTracks))
            options.AddRange(vanillaTracks);

        var random = Random.Next(0, options.Count);
        return options[random];
    }

    /// <summary>
    /// Obtains all potential candidate tracks for a given stage.
    /// </summary>
    /// <param name="stageId">The stage index.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    public void GetTracksForStage(int stageId, List<string> files) => GetFilesForFileName($"STG{stageId:00}.adx", files);
}
