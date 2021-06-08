using System;
using System.Collections.Generic;
using System.IO;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Enums;
using Riders.Tweakbox.Services.Texture.Structs;

namespace Riders.Tweakbox.Services.Texture
{
    /// <summary>
    /// Watches over a specified folder and subfolders for PNG and DDS files in real time.
    /// </summary>
    public class TextureDictionary
    {
        // DO NOT CHANGE
        private const int HashLength = 16;

        /// <summary>
        /// The path to the folder where textures are sourced from.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Maps texture hashes to new file paths.
        /// </summary>
        private Dictionary<string, TextureFile> Redirects { get; set; } = new Dictionary<string, TextureFile>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Maps texture hashes to animated textures.
        /// </summary>
        private Dictionary<string, AnimatedTexture> AnimatedRedirects { get; set; } = new Dictionary<string, AnimatedTexture>(StringComparer.OrdinalIgnoreCase);

        private FileSystemWatcher _watcher;

        /// <summary/>
        /// <param name="source">Directory containing textures in PNG or DDS format.</param>
        public TextureDictionary(string source)
        {
            Source = source;
            SetupFileWatcher();
            SetupRedirects();
        }

        /// <summary>
        /// Tries to pull a texture from this dictionary.
        /// Texture data is placed in the resulting output span.
        /// </summary>
        /// <param name="xxHash">The hash.</param>
        /// <param name="data">The texture data.</param>
        /// <param name="info">The file path from which the data was loaded.</param>
        public bool TryGetTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            if (TryGetNormalTexture(xxHash, out data, out info))
                return true;

            if (TryGetAnimatedTexture(xxHash, out data, out info))
                return true;

            info = default;
            data = default;
            return false;
        }

        private bool TryGetAnimatedTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            var animRedirects = AnimatedRedirects;

            if (animRedirects.TryGetValue(xxHash, out var animTexture))
            {
                data = animTexture.GetFirstTexture(out var filePath);
                info = new TextureInfo(filePath, TextureType.Animated, animTexture);
                return true;
            }

            info = default;
            data = default;
            return false;
        }

        private bool TryGetNormalTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            var fileRedirects = Redirects;

            if (fileRedirects.TryGetValue(xxHash, out var redirect))
            {
                data = TextureRef.FromFile(redirect.Path, redirect.Format);
                info = new TextureInfo(redirect.Path, TextureType.Normal);
                return true;
            }

            info = default;
            data = default;
            return false;
        }

        private void SetupFileWatcher()
        {
            _watcher = FileSystemWatcherExtensions.Create(Source, new []
            {
                TextureCommon.PngFilter, 
                TextureCommon.DdsFilter, 
                TextureCommon.DdsLz4Filter
            }, SetupRedirects);
        }

        private void SetupRedirects()
        {
            if (!Directory.Exists(Source))
                return;
                
            SetupFileRedirects();
            SetupFolderRedirects();
        }

        private void SetupFileRedirects()
        {
            var redirects = new Dictionary<string, TextureFile>(StringComparer.OrdinalIgnoreCase);
            var allFiles  = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                var type = file.GetTextureFormatFromFileName();
                if (type == TextureFormat.Unknown)
                    continue;

                // Extract hash from filename.
                var fileName = Path.GetFileName(file);
                var indexOfHash = fileName.IndexOf('_');
                if (indexOfHash == -1)
                    continue;

                string hash = fileName.Substring(indexOfHash + 1, HashLength);
                redirects[hash] = new TextureFile() {Path = file, Format = type};
            }

            Redirects = redirects;
        }

        private void SetupFolderRedirects()
        {
            var redirects  = new Dictionary<string, AnimatedTexture>(StringComparer.OrdinalIgnoreCase);
            var allFolders = Directory.GetDirectories(Source, "*.*", SearchOption.AllDirectories);

            foreach (string folder in allFolders)
            {
                // Extract hash from filename.
                var folderName = Path.GetFileName(folder);
                var indexOfHash = folderName.IndexOf('_');
                if (indexOfHash == -1)
                    continue;

                try
                {
                    string hash = folderName.Substring(indexOfHash + 1, HashLength);
                    if (AnimatedTexture.TryCreate(folder, out var texture))
                        redirects[hash] = texture;
                }
                catch (Exception) { }
            }

            AnimatedRedirects = redirects;
        }
    }
}
