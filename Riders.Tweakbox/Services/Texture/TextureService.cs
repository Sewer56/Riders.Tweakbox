using System;
using System.Collections.Generic;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Standart.Hash.xxHash;

namespace Riders.Tweakbox.Services.Texture
{
    /// <summary>
    /// The texture service keeps track of injectable textures sourced from other mods.
    /// </summary>
    public class TextureService
    {
        // DO NOT CHANGE, TEXTUREDICTIONARY DEPENDS ON THIS.
        private const string HashStringFormat = "X8";

        private List<TextureDictionary> _dictionaries = new List<TextureDictionary>();
        private IModLoader _modLoader;

        public TextureService(IModLoader modLoader)
        {
            _modLoader = modLoader;
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
        /// Gets a hash for a single texture.
        /// </summary>
        /// <param name="textureData">Span which contains only the texture data..</param>
        public string ComputeHashString(Span<byte> textureData)
        {
            // Alternative seed for hashing, must not be changed.
            const int AlternateSeed = 1337;

            /*
                According to the birthday attack, you need around 2^(n/2) hashes before a collision.
                As such, using a 32-bit xxHash alone would mean that you only need 2^16 hashes until you
                are to expect a collision which is insufficient.
            
                Problem is this is a 32-bit process and the 64-bit version of the algorithm is 10x slower;
                so in order to edge out at least a tiny bit more of collision resistance, we hash the file twice
                with a different seed, hoping the combined seed should be more collision resistant.
                
                This should work as hash functions are designed to have an avalanche effect; changing the seed should
                drastically change the hash and resolve collisions for previously colliding files (I tested this).

                While the world is not ideal and hash functions aren't truly random; probability theory suggests that 
                P(AnB) = P(A) x P(B), which in our case is 2^16 x 2^16, which is 2^32. I highly doubt Riders has 
                anywhere near 4,294,967,296 textures.

                When I tested this with a lowercase English dictionary of 479k words; I went from 14 collisions to 0.
            */

            var xxHashA = xxHash32.ComputeHash(textureData, textureData.Length);
            var xxHashB = xxHash32.ComputeHash(textureData, textureData.Length, AlternateSeed);
            return xxHashA.ToString(HashStringFormat) + xxHashB.ToString(HashStringFormat);
        }

        /// <summary>
        /// Gets the data for a specific texture.
        /// </summary>
        /// <param name="xxHash">Hash of the texture that was loaded.</param>
        /// <param name="data">The loaded texture data.</param>
        /// <param name="filePath">File path of the texture that was loaded.</param>
        /// <returns>Whether texture data was found.</returns>
        public bool TryGetData(string xxHash, out TextureRef data, out string filePath)
        {
            // Doing this in reverse because mods with highest priority get loaded last.
            // We want to look at those mods first.
            for (int i = _dictionaries.Count - 1; i >= 0; i--)
            {
                if (_dictionaries[i].TryGetTexture(xxHash, out data, out filePath))
                    return true;
            }

            filePath = default;
            data     = default;
            return false; 

        }

        private void Add(IModConfigV1 config)
        {
            _dictionaries.Add(new TextureDictionary(GetRedirectFolder(config.ModId)));
        }

        private void Remove(IModConfigV1 config)
        {
            var redirectFolder = GetRedirectFolder(config.ModId);
            _dictionaries = _dictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
        private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);
        private string GetRedirectFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/Textures";
    }
}