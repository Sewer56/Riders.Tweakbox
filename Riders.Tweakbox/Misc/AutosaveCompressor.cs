using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Utility class for compressing old autosaves.
    /// </summary>
    public class AutosaveCompressor : IDisposable
    {
        private ZipArchive _archive;
        private FileStream _fileStream;
        private string _logArchive;

        /// <summary>
        /// Opens a zip archive for writing.
        /// </summary>
        /// <param name="logArchive">Path to the archive used for storing old autosaves.</param>
        public AutosaveCompressor(string logArchive)
        {
            _logArchive = logArchive;
            Directory.CreateDirectory(Path.GetDirectoryName(logArchive));
            _fileStream = new FileStream(_logArchive, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _archive = new ZipArchive(_fileStream, ZipArchiveMode.Update, true);
        }

        /// <summary>
        /// Releases resources related to this compressor.
        /// </summary>
        public void Dispose()
        {
            _fileStream?.Dispose();
            _archive?.Dispose();
        }

        /// <summary>
        /// Force an update of the physical file contents.
        /// </summary>
        public void Flush()
        {
            // ZipArchive has no flush method, so dispose is the only way for now.
            _archive?.Dispose();
            _fileStream?.Flush();
            _archive = new ZipArchive(_fileStream, ZipArchiveMode.Update, true);
        }

        /// <summary>
        /// Deletes files from the archive older than the specified amount of time (since last modified).
        /// </summary>
        /// <param name="span">The maximum amount of time to keep files. Files older will be removed.</param>
        public void DeleteOldFiles(TimeSpan span)
        {
            var now = DateTime.Now;
            foreach (var entry in _archive.Entries.ToArray())
            {
                var timeSince = now - entry.LastWriteTime;
                if (timeSince > span)
                {
                    entry.Delete();
                }
            }
        }

        /// <summary>
        /// Adds all of the files from a given folder into the archive and deletes the original files.
        /// </summary>
        /// <param name="folderPath">The folder inside which to scan for files.</param>
        /// <param name="span">The minimum amount of time since last modified for files to be added.</param>
        /// <param name="filter">File name filter for the files to be added.</param>
        public void AddFiles(string folderPath, TimeSpan span, string filter = "*")
        {
            var files = Directory.GetFiles(folderPath, filter);
            var now = DateTime.Now;

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var timeSince = now - info.LastWriteTime;
                if (timeSince > span)
                {
                    _archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    File.Delete(file);
                }
            }
        }
    }
}
