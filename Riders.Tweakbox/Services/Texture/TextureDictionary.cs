using System;
using System.Collections.Generic;
using System.IO;

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
        public string Source;

        /// <summary>
        /// Maps texture hashes to new file paths.
        /// </summary>
        private Dictionary<string, Redirect> Redirects { get; set; } = new Dictionary<string,Redirect>(StringComparer.OrdinalIgnoreCase);

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
        public bool TryGetTexture(string xxHash, out TextureRef data, out string filePath)
        {
            var fileRedirects = Redirects;
            if (fileRedirects.TryGetValue(xxHash, out var redirect))
            {
                filePath = redirect.FilePath;
                if (redirect.Type == RedirectType.DdsLz4)
                {
                    data = TextureCompression.PickleFromFile(filePath);
                    return true;
                }

                data = new TextureRef(File.ReadAllBytes(filePath));
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
                _watcher.Filters.Add(TextureCommon.PngFilter);
                _watcher.Filters.Add(TextureCommon.DdsFilter);
                _watcher.Filters.Add(TextureCommon.DdsLz4Filter);

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
                var redirects   = new Dictionary<string, Redirect>(StringComparer.OrdinalIgnoreCase);
                var allFiles    = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);
                
                foreach (string file in allFiles)
                {
                    var type = GetRedirectType(file);
                    if (type == RedirectType.None)
                        continue;

                    // Extract hash from filename.
                    var indexOfHash = file.IndexOf('_');
                    if (indexOfHash == -1)
                        continue;

                    string hash = file.Substring(indexOfHash + 1, HashLength);
                    redirects[hash] = new Redirect() { FilePath = file, Type = type };
                }

                Redirects = redirects;
            }
        }

        private RedirectType GetRedirectType(string file)
        {
            if (file.EndsWith(TextureCommon.DdsExtension, StringComparison.OrdinalIgnoreCase))
                return RedirectType.Dds;

            if (file.EndsWith(TextureCommon.PngExtension, StringComparison.OrdinalIgnoreCase))
                return RedirectType.Png;

            if (file.EndsWith(TextureCommon.DdsLz4Extension, StringComparison.OrdinalIgnoreCase))
                return RedirectType.DdsLz4;

            return RedirectType.None;
        }

        private struct Redirect
        {
            public string FilePath;
            public RedirectType Type;
        }

        private enum RedirectType
        {
            None,
            Png,
            Dds,
            DdsLz4
        }
    }
}
