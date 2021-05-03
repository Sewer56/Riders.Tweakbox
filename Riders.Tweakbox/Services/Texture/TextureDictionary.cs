using System;
using System.Collections.Generic;
using System.IO;
using StructLinq;

namespace Riders.Tweakbox.Services.Texture
{
    /// <summary>
    /// Watches over a specified folder and subfolders for PNG and DDS files in real time.
    /// </summary>
    public class TextureDictionary
    {
        private const string PngExtension = ".png";
        private const string DdsExtension = ".dds";

        private const string PngFilter = "*.png";
        private const string DdsFilter = "*.dds";

        // DO NOT CHANGE
        private const int HashLength = 16;

        /// <summary>
        /// Maps texture hashes to new file paths.
        /// </summary>
        public Dictionary<string, string> Redirects { get; set; } = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The path to the folder where textures are sourced from.
        /// </summary>
        public string Source;

        private FileSystemWatcher _watcher;

        /// <summary/>
        /// <param name="source">Directory containing textures in PNG or DDS format.</param>
        public TextureDictionary(string source)
        {
            Source = source;
            SetupFileWatcher();
            SetupFileRedirects();
        }

        /// <summary>
        /// Tries to pull a texture from this dictionary.
        /// Texture data is placed in the resulting output span.
        /// </summary>
        /// <param name="xxHash">The hash.</param>
        /// <param name="data">The texture data.</param>
        /// <param name="filePath">The file path from which the data was loaded.</param>
        public bool TryGetTexture(string xxHash, out Span<byte> data, out string filePath)
        {
            var fileRedirects = Redirects;
            if (fileRedirects.TryGetValue(xxHash, out filePath))
            {
                data = File.ReadAllBytes(filePath);
                return true;
            }

            filePath = default;
            data     = default;
            return false;
        }

        private void SetupFileWatcher()
        {
            if (Directory.Exists(Source))
            {
                _watcher = new FileSystemWatcher();
                _watcher.Path = Source;
                _watcher.Filters.Add(PngFilter);
                _watcher.Filters.Add(DdsFilter);

                _watcher.EnableRaisingEvents   = true;
                _watcher.IncludeSubdirectories = true;
                _watcher.Created += (sender, args) => { SetupFileRedirects(); };
                _watcher.Deleted += (sender, args) => { SetupFileRedirects(); };
                _watcher.Renamed += (sender, args) => { SetupFileRedirects(); };
            }
        }

        private void SetupFileRedirects()
        {
            if (Directory.Exists(Source))
            {
                var redirects   = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var allFiles    = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);
                var allTextures = allFiles.ToStructEnumerable().Where(x => x.EndsWith(PngExtension) || x.EndsWith(DdsExtension), x => x);
                
                foreach (string textureFile in allTextures)
                {
                    // Extract hash from filename.
                    var indexOfHash = textureFile.IndexOf('_');
                    if (indexOfHash == -1)
                        continue;

                    string hash = textureFile.Substring(indexOfHash + 1, HashLength);
                    redirects[hash] = textureFile;
                }

                Redirects = redirects;
            }
        }
    }
}
