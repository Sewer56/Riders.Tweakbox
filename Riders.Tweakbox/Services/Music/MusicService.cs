using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;

namespace Riders.Tweakbox.Services.Music
{
    /// <summary>
    /// Keeps track of all music tracks provided by other mods (as well as the vanilla game)
    /// </summary>
    public class MusicService : ISingletonService
    {
        private List<MusicDictionary> _dictionaries = new List<MusicDictionary>();
        private MusicDictionary _vanillaDict;
        private IModLoader _modLoader;
        private Random _random = new Random();

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
        /// <returns>Path to the replacement track..</returns>
        public string GetRandomTrack(string fileName, bool includeVanilla)
        {
            var options = new List<string>();
            if (includeVanilla && _vanillaDict.TryGetTrack(fileName, out var vanillaTracks))
                options.AddRange(vanillaTracks);

            foreach (var dictionary in _dictionaries)
            {
                if (dictionary.TryGetTrack(fileName, out var modTracks))
                    options.AddRange(modTracks);
            }

            var random = _random.Next(0, options.Count);
            return options[random];
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
