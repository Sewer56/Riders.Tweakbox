using System;
using System.Collections.Generic;
using System.IO;
using Riders.Tweakbox.Misc.Extensions;

namespace Riders.Tweakbox.Services.Music
{
    /// <summary>
    /// Watches over a specified folder and subfolders for ADX files in real time.
    /// Creates a map of file name to list of files.
    /// </summary>
    public class MusicDictionary
    {
        /// <summary>
        /// The path to the folder where tracks are sourced from.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Maps file names to new file paths.
        /// </summary>
        private Dictionary<string, List<string>> Files { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private FileSystemWatcher _watcher;

        /// <summary/>
        /// <param name="source">Directory containing textures in PNG or DDS format.</param>
        public MusicDictionary(string source)
        {
            Source = source;
            SetupFileWatcher();
            SetupRedirects();
        }

        /// <summary>
        /// Tries to find a replacement for a music track.
        /// </summary>
        /// <param name="fileName">The name of the file to get.</param>
        /// <param name="paths">The list of tracks with matching names in the search directory.</param>
        public bool TryGetTrack(string fileName, out List<string> paths)
        {
            return Files.TryGetValue(fileName, out paths);
        }

        private void SetupFileWatcher()
        {
            _watcher = FileSystemWatcherExtensions.Create(Source, new [] { MusicCommon.AdxFilter }, SetupRedirects);
        }

        private void SetupRedirects()
        {
            if (!Directory.Exists(Source))
                return;

            var files    = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var allFiles = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                var extension = Path.GetExtension(file);
                if (!extension.Equals(MusicCommon.AdxExtension, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract hash from filename.
                var fileName = Path.GetFileName(file);
                if (!files.TryGetValue(fileName, out var fileList))
                    fileList = new List<string>();

                // Game does not like forward slashes :/
                fileList.Add(file.Replace('/', '\\')); 
                files[fileName] = fileList;
            }

            Files = files;
        }
    }
}
