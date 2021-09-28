using Riders.Tweakbox.Misc.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Riders.Tweakbox.Misc.Extensions;

public static class DirectoryExtensions
{
    /// <summary>
    /// Copies the contents of a directory from one directory to another.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The destination directory.</param>
    /// <param name="recursive">Copies subdirectories.</param>
    public static void DirectoryCopy(string source, string destination, bool recursive)
    {
        // Get the subdirectories and c
        var dir = new DirectoryInfo(source);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {source}");

        Directory.CreateDirectory(destination);
        var dirs = dir.GetDirectories();
        
        // Get the files in the directory and copy them to the new location.
        var files = dir.GetFiles();
        foreach (var file in files)
            file.CopyTo(Path.Combine(destination, file.Name), true);

        // If copying subdirectories, copy them and their contents to new location.
        if (!recursive)
            return;

        foreach (var subdir in dirs)
            DirectoryCopy(subdir.FullName, Path.Combine(destination, subdir.Name), recursive);
    }

    /// <summary>
    /// Finds a file with a specific name (no extension) inside a directory.
    /// </summary>
    /// <param name="directory">The directory to look for the file in.</param>
    /// <param name="name">Name of the file, without extension.</param>
    /// <returns>Path to the file, else null.</returns>
    public static string GetFileWithName(string directory, string name)
    {
        if (!DirectorySearcher.TryGetDirectoryContents(directory, out var files, out var directories))
            return null;

        return GetFileWithName(files, name);
    }

    /// <summary>
    /// Finds a file with a specific name (no extension) inside a directory.
    /// </summary>
    /// <param name="files">Information about the files in the directory.</param>
    /// <param name="name">Name of the file, without extension.</param>
    /// <returns>Path to the file, else null.</returns>
    public static string GetFileWithName(List<DirectorySearcher.FileInformation> files, string name)
    {
        foreach (var file in files)
        {
            if (Path.GetFileNameWithoutExtension(file.FullPath) == name)
                return file.FullPath;
        }

        return null;
    }
}
