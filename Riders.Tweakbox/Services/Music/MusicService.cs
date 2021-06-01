using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Services.Music
{
    /// <summary>
    /// Keeps track of all music tracks provided by other mods (as well as the vanilla game)
    /// </summary>
    public class MusicService : ISingletonService
    {
        private static HashSet<string> _vanillaStageTracks = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "S1.adx", "S2.adx", "S3.adx", "S4.adx", "S5.adx", "S6.adx", "S7.adx", "S8.adx", "SV1.adx" };

        private List<MusicDictionary> _dictionaries = new List<MusicDictionary>();
        private MusicDictionary _vanillaDict;
        private IModLoader _modLoader;

        public MusicService(IModLoader modLoader)
        {
            _vanillaDict = new MusicDictionary(Path.Combine(IO.GameFolderLocation, "Data"));
            _modLoader   = modLoader;
            _modLoader.ModLoading   += OnModLoading;
            _modLoader.ModUnloading += OnModUnloading;

            // Fill in textures from already loaded mods.
            var existingMods = _modLoader.GetActiveMods().Select(x => x.Generic);
            foreach (var mod in existingMods)
            {
                Add(mod);
            }
        }

        /// <summary>
        /// Gets the data for a specific texture.
        /// </summary>
        /// <param name="fileName">The file name for which to get a replacement track.</param>
        /// <param name="includeVanilla">Whether to include vanilla tracks or not.</param>
        /// <param name="includePerStageTracks">Whether to include stage-specific tracks.</param>
        /// <returns>Path to the replacement track.</returns>
        public unsafe string GetRandomTrack(string fileName, bool includeVanilla, bool includePerStageTracks)
        {
            var options = new List<string>();
            if (includeVanilla && _vanillaDict.TryGetTrack(fileName, out var vanillaTracks))
                options.AddRange(vanillaTracks);

            if (includePerStageTracks && _vanillaStageTracks.Contains(fileName))
                GetTracksForStage(*(int*)State.Level, options);

            GetTracksForFileName(fileName, options);
            var random = Misc.Extensions.SharedRandom.Instance.Next(0, options.Count);
            return options[random];
        }

        /// <summary>
        /// Obtains all potential candidate tracks for a given stage.
        /// </summary>
        /// <param name="stageId">The stage index.</param>
        /// <param name="files">List of files to add the candidates to.</param>
        public void GetTracksForStage(int stageId, List<string> files) => GetTracksForFileName($"STG{stageId:00}.adx", files);

        /// <summary>
        /// Obtains all potential candidate tracks for a given file name.
        /// </summary>
        /// <param name="fileName">The name of the file to get the tracks for</param>
        /// <param name="files">List of files to add the candidates to.</param>
        public void GetTracksForFileName(string fileName, List<string> files)
        {
            foreach (var dictionary in _dictionaries)
            {
                if (dictionary.TryGetTrack(fileName, out var modTracks))
                    files.AddRange(modTracks);
            }
        }

        private void Add(IModConfigV1 config) => _dictionaries.Add(new MusicDictionary(GetRedirectFolder(config.ModId)));
        private void Remove(IModConfigV1 config)
        {
            var redirectFolder = GetRedirectFolder(config.ModId);
            _dictionaries = _dictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
        private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);
        private string GetRedirectFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/Music";
    }
}
