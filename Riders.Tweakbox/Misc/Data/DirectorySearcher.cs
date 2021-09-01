﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using EnumsNET;
using Microsoft.Windows.Sdk;

namespace Riders.Tweakbox.Misc.Data;


/// <summary>
/// Class that provides WinAPI based utility methods for fast file enumeration in directories.
/// </summary>
public static class DirectorySearcher
{
    private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    /// <summary>
    /// Retrieves the total contents of a directory.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation suceeded, else false.</returns>
    public static bool TryGetDirectoryContents(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        // Init
        path = Path.GetFullPath(path);
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        
        // Native Init
        WIN32_FIND_DATAW findData;
        var findHandle = PInvoke.FindFirstFile($@"{path}\*", out findData);
        if (findHandle == INVALID_HANDLE_VALUE)
            return false;
        
        do
        {
            // Get each file name subsequently.
            var fileName = findData.GetFileName();
            if (fileName == "." || fileName == "..") 
                continue;

            string fullPath = $@"{path}\{fileName}";

            // Check if this is a directory and not a symbolic link since symbolic links
            // could lead to repeated files and folders as well as infinite loops.
            var attributes = (FileAttributes) findData.dwFileAttributes;
            bool isDirectory = attributes.HasAllFlags(FileAttributes.Directory);

            if (isDirectory && !attributes.HasAllFlags(FileAttributes.ReparsePoint))
            {
                directories.Add(new DirectoryInformation
                {
                    FullPath = fullPath, 
                    LastWriteTime = findData.ftLastWriteTime.ToDateTime()
                });
            }
            else if (!isDirectory)
            {
                files.Add(new FileInformation
                {
                    FullPath = fullPath, 
                    LastWriteTime = findData.ftLastWriteTime.ToDateTime()
                });
            }
        }
        while (FindNextFile(findHandle, out findData));

        if (findHandle != INVALID_HANDLE_VALUE)
            PInvoke.FindClose((HANDLE) findHandle.Value);
        
        return true;
    }

    
    public struct FileInformation
    {
        public string FullPath;
        public DateTime LastWriteTime;
    }

    public struct DirectoryInformation
    {
        public string FullPath;
        public DateTime LastWriteTime;
    }

    // The import from Microsoft.Windows.Sdk uses a class as paramter; don't want heap allocations.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);
}

/// <summary>
/// Extensions to the automatically Source Generator imported WinAPI classes.
/// </summary>
public static class FindDataExtensions
{
    internal static unsafe string GetFileName(this WIN32_FIND_DATAW value)
    {
        fixed (ushort* data = value.cFileName)
            return Marshal.PtrToStringUni((IntPtr)data);
    }
}

/// <summary>
/// Extensions to the internal COM FILETIME class.
/// </summary>
public static class FileTimeExtensions
{
    public static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME time)
    {
        ulong high = (ulong)time.dwHighDateTime;
        ulong low  = (ulong)time.dwLowDateTime;
        long fileTime = (long)((high << 32) + low);
        return DateTime.FromFileTimeUtc(fileTime);
    }
}