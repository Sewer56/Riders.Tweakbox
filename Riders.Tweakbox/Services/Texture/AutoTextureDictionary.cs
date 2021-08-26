using System;
using System.Collections.Generic;
using System.IO;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Interfaces;
using Riders.Tweakbox.Services.Texture.Structs;
namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// Automatic implementation of a texture dictionary that watches
/// over a specified folder and subfolders for PNG and DDS files in real time.
/// </summary>
public class AutoTextureDictionary : TextureDictionaryBase
{
    /// <summary>
    /// The path to the folder where textures are sourced from.
    /// </summary>
    public string Source { get; private set; }
    private FileSystemWatcher _watcher;

    /// <summary/>
    /// <param name="source">Directory containing textures in PNG or DDS format.</param>
    public AutoTextureDictionary(string source)
    {
        Source = source;
        SetupFileWatcher();
        SetupRedirects();
    }

    private void SetupFileWatcher()
    {
        _watcher = FileSystemWatcherExtensions.Create(Source, new[]
        {
            TextureCommon.PngFilter,
            TextureCommon.DdsFilter,
            TextureCommon.DdsLz4Filter
        }, SetupRedirects);
    }

    private void SetupRedirects()
    {
        if (!Directory.Exists(Source))
            return;

        SetupFileRedirects();
        SetupFolderRedirects();
    }

    private void SetupFileRedirects()
    {
        var redirects = new Dictionary<string, TextureFile>(StringComparer.OrdinalIgnoreCase);
        var allFiles = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            if (TryAddTextureFromFilePath(file, out var result, out var hash))
                redirects[hash] = result;
        }

        Redirects = redirects;
    }

    private void SetupFolderRedirects()
    {
        var redirects = new Dictionary<string, AnimatedTexture>(StringComparer.OrdinalIgnoreCase);
        var allFolders = Directory.GetDirectories(Source, "*.*", SearchOption.AllDirectories);

        foreach (string folder in allFolders)
        {
            if (TryMakeAnimatedTextureFromFolder(folder, out var result, out var hash))
                redirects[hash] = result;
        }

        AnimatedRedirects = redirects;
    }
}
