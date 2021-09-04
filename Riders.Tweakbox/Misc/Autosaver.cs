using System;
using System.IO;
using System.Threading.Tasks;

namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Helper class for working with autosaves; where every autosave is a single file in a common directory.
    /// </summary>
    public class Autosaver
    {
        /// <summary>
        /// Amount of time in hours since last modified that files get compressed.
        /// </summary>
        public TimeSpan CompressTime { get; set; }

        /// <summary>
        /// Amount of time in hours since last modified that files get deleted.
        /// </summary>
        public TimeSpan DeleteTime { get; set; }

        /// <summary>
        /// The amount of time between each autosave.
        /// Use <see cref="Update"/> to determine if a save needs to be done.
        /// </summary> 
        public TimeSpan AutosaveInterval { get; set; }

        /// <summary>
        /// The directory this helper works with.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// The time the last autosave has happened.
        /// I.e. last time <see cref="Update"/> returned true.
        /// </summary>
        public DateTime LastAutosave { get; private set; }

        /// <summary>
        /// The time the last compress has happened.
        /// I.e. last time <see cref="ForceCompress"/> was called.
        /// </summary>
        public DateTime LastCompress { get; private set; }

        /// <summary>
        /// The filter used by the file archiver.
        /// </summary>
        public string Filter { get; private set; }

        private AutosaveCompressor _autosaveCompressor;

        private Autosaver() { }
        public Autosaver(string directory, TimeSpan autosaveInterval, TimeSpan compressTime, TimeSpan deleteTime, string archiverFilter = "*.txt")
        {
            CompressTime = compressTime;
            DeleteTime = deleteTime;
            Directory = directory;
            AutosaveInterval = autosaveInterval;
            Filter = archiverFilter;
            _autosaveCompressor = new AutosaveCompressor(Path.Combine(Directory, "old.zip"));
        }

        /// <summary>
        /// Polls whether an autosave should be made.
        /// </summary>
        /// <param name="compressionPerformed">True if compression has been performed.</param>
        public bool Update(out bool compressionPerformed)
        {
            var currentTime = DateTime.UtcNow;

            bool shouldAutosave  = currentTime - LastAutosave > AutosaveInterval;
            compressionPerformed = currentTime - LastCompress > CompressTime;

            if (shouldAutosave)
                LastAutosave = currentTime;

            if (compressionPerformed)
                ForceCompress();

            return shouldAutosave;
        }

        /// <summary>
        /// Performs compression on old files and removal on too old files.
        /// </summary>
        public void ForceCompress()
        {
            LastCompress = DateTime.UtcNow;
            Task.Run(() =>
            {
                _autosaveCompressor.AddFiles(Directory, CompressTime, Filter);
                _autosaveCompressor.DeleteOldFiles(DeleteTime);
                _autosaveCompressor.Flush();
            }).ConfigureAwait(false);
        }
    }
}
