using System;
using System.Collections.Generic;
using System.IO;
using DotNext.Collections.Generic;
namespace Riders.Tweakbox.Misc.Extensions;

public static class FileSystemWatcherExtensions
{
    /// <summary>
    /// Creates a <see cref="FileSystemWatcher"/> that watches all changes to
    /// the file contents within a specified directory.
    /// Works on Create, Rename, Delete.
    /// </summary>
    /// <param name="folder">The folder to watch.</param>
    /// <param name="filters">The file types to watch.</param>
    /// <param name="action">Action to execute on detected change.</param>
    /// <returns>Null if failed or directory did not exist.</returns>
    public static FileSystemWatcher Create(string folder, IEnumerable<string> filters, Action action)
    {
        if (!Directory.Exists(folder))
            return null;

        var watcher = new FileSystemWatcher();
        watcher.Path = folder;

        watcher.Filters.AddAll(filters);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.Created += (sender, args) => { action(); };
        watcher.Deleted += (sender, args) => { action(); };
        watcher.Renamed += (sender, args) => { action(); };
        return watcher;
    }

}
