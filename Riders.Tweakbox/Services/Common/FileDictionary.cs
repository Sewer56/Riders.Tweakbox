using System;
using System.Collections.Generic;
using System.IO;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Music;

namespace Riders.Tweakbox.Services.Common;

/// <summary>
/// Watches over a specified folder and subfolders for files with a specified extension in real time.
/// Creates a map of file name to list of files.
/// </summary>
public class FileDictionary
{
    /// <summary>
    /// The path to the folder where files are sourced from.
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// The filter this dictionary was created with, e.g. "*.adx"
    /// </summary>
    public string Extension { get; private set; }

    /// <summary>
    /// Maps file names to new file paths.
    /// </summary>
    private Dictionary<string, List<string>> Files { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    private FileSystemWatcher _watcher;

    public FileDictionary() { }

    /// <summary/>
    /// <param name="source">Path to the folder where files are sourced from.</param>
    /// <param name="extension">The file extension tracked, e.g. ".adx".</param>
    public FileDictionary(string source, string extension) => InitializeBase(source, extension);

    /// <summary>
    /// Initializes the file dictionary using a given source and the default extension.
    /// </summary>
    /// <param name="source">Path to the folder where files are sourced from.</param>
    public virtual void Initialize(string source) => throw new Exception("Please override this from a derived class.");

    protected void InitializeBase(string source, string extension)
    {
        Source = source;
        Extension = extension;
        SetupFileWatcher();
        SetupRedirects();
    }

    /// <summary>
    /// Tries to find a replacement for a given file.
    /// </summary>
    /// <param name="fileName">The name of the file to get.</param>
    /// <param name="paths">The list of files with matching names in the search directory.</param>
    public bool TryGetValue(string fileName, out List<string> paths)
    {
        return Files.TryGetValue(fileName, out paths);
    }

    /// <summary>
    /// Returns true if has at least 1 element.
    /// </summary>
    public bool Any() => Files.Count > 0;

    private void SetupFileWatcher()
    {
        _watcher = FileSystemWatcherExtensions.Create(Source, new[] { $"*{Extension}" }, SetupRedirects);
    }

    private void SetupRedirects()
    {
        if (!Directory.Exists(Source))
            return;

        var files = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var allFiles = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            var extension = Path.GetExtension(file);
            if (!extension.Equals(Extension, StringComparison.OrdinalIgnoreCase))
                continue;

            // Add file name to list if necessary.
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
