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
            file.CopyTo(Path.Combine(destination, file.Name), false);

        // If copying subdirectories, copy them and their contents to new location.
        if (!recursive)
            return;

        foreach (var subdir in dirs)
            DirectoryCopy(subdir.FullName, Path.Combine(destination, subdir.Name), recursive);
    }

}
