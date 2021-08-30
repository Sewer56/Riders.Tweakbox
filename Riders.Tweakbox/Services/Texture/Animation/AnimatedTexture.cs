﻿using System;
using System.Collections.Generic;
using System.IO;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Data;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Texture.Enums;
using Riders.Tweakbox.Services.Texture.Structs;
using SharpDX.Direct3D9;
namespace Riders.Tweakbox.Services.Texture.Animation;

public class AnimatedTexture : IDisposable
{
    /// <summary>
    /// The folder where the texture set resides.
    /// </summary>
    public string Folder { get; private set; }

    /// <summary>
    /// List of files associated with the folder.
    /// </summary>
    public List<AnimatedTextureFile> Files { get; private set; }

    /// <summary>
    /// Individual loaded in textures.
    /// </summary>
    public List<SharpDX.Direct3D9.Texture> Textures { get; private set; }

    private int _modulo = 0;
    private List<int> _minId;
    private bool _loaded;
    private int _lastIndex;

    private AnimatedTexture() { }

    ~AnimatedTexture() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var texture in Textures)
            texture?.Dispose();

        Textures.Clear();
        _loaded = false;
    }

    /// <summary>
    /// Gets a reference to the first texture to display.
    /// </summary>
    /// <param name="filePath">The path to the first texture.</param>
    public void GetFirstTexturePath(out string filePath)
    {
        var file = Files[^1];
        filePath = Folder + file.RelativePath;
    }

    /// <summary>
    /// Gets a reference to the first texture to display.
    /// </summary>
    /// <param name="filePath">The path to the first texture.</param>
    public TextureRef GetFirstTexture(out string filePath)
    {
        var file = Files[^1];
        filePath = Folder + file.RelativePath;
        return TextureRef.FromFile(filePath, file.Format);
    }

    /// <summary>
    /// Pre-loads all textures into memory.
    /// </summary>
    /// <param name="firstTexReference">Native pointer to the first texture instance.</param>
    public unsafe void Preload(void* firstTexReference)
    {
        if (_loaded)
            return;

        var device = IoC.Get<Device>();
        Textures.Add(new SharpDX.Direct3D9.Texture((IntPtr)firstTexReference));
        for (var x = 0; x < Files.Count - 1; x++)
        {
            var file = Files[x];
            var fullPath = Folder + file.RelativePath;
            var texRef = TextureRef.FromFile(fullPath, file.Format);
            var texture = SharpDX.Direct3D9.Texture.FromMemory(device, texRef.Data, Usage.Dynamic, Pool.Default);
            if (texRef.ShouldBeCached())
            {
                var cache = TextureCacheService.Instance;
                cache?.QueueStore(fullPath, texture);
            }

            Textures.Add(texture);
        }
        
        _loaded = true;
    }

    /// <summary>
    /// Gets a texture for a given frame.
    /// </summary>
    /// <param name="currentFrame">The current rendering frame.</param>
    public unsafe void* GetTextureForFrame(int currentFrame)
    {
        currentFrame %= _modulo;
        int currentIndex = _lastIndex;
        int maxIndex = _minId.Count - 1;

        // Optimization if last index was last item
        // Check between 0 and first index
        if (currentIndex == maxIndex)
        {
            // Check if still on last.
            if (currentFrame < _minId[0])
                return (void*)Textures[_lastIndex].NativePointer;

            // Else start from first element.
            currentIndex = 0;
        }

        // Loop circular once over until found.
        // Typically loop takes 1 iteration.
        do
        {
            var current = _minId[currentIndex];
            var next    = _minId[currentIndex + 1];

            if (currentFrame >= current && currentFrame < next)
            {
                _lastIndex = currentIndex;
                return (void*)Textures[currentIndex].NativePointer;
            }

            if (currentFrame == next)
            {
                _lastIndex = currentIndex + 1;
                return (void*)Textures[currentIndex + 1].NativePointer;
            }

            currentIndex++;
            currentIndex %= maxIndex;
        }
        while (currentIndex != _lastIndex);

        return (void*)Textures[_lastIndex].NativePointer;
    }

    /// <summary>
    /// Tries to create a texture
    /// Creation fails if there is not a single texture in the folder.
    /// </summary>
    /// <param name="folderPath">Path to the texture folder.</param>
    /// <param name="texture">The texture.</param>
    public static bool TryCreate(string folderPath, out AnimatedTexture texture)
    {
        try
        {
            texture = new AnimatedTexture();
            texture.Folder = Path.GetFullPath(folderPath);
            return texture.TryLoad();
        }
        catch (Exception e)
        {
            Log.WriteLine($"Failed to make AnimatedTexture: Folder | {folderPath} | {e.Message}");
            texture = null;
            return false;
        }
    }

    /// <summary>
    /// Tried to reload this animated texture's contents.
    /// </summary>
    public bool TryReload()
    {
        Dispose();
        return TryLoad();
    }

    private bool TryLoad()
    {
        var files = Directory.GetFiles(Folder, "*.*", SearchOption.TopDirectoryOnly);
        Files     = new List<AnimatedTextureFile>(files.Length);
        _minId    = new List<int>(files.Length);
        Textures  = new List<SharpDX.Direct3D9.Texture>();

        foreach (var file in files)
        {
            var format = file.GetTextureFormatFromFileName();
            if (format == TextureFormat.Unknown)
                continue;

            var fullPath = Path.GetFullPath(file);
            var relativePath = fullPath.Substring(Folder.Length);
            Files.Add(new AnimatedTextureFile(relativePath, format));
        }

        // Sort
        Files.Sort((first, second) => string.CompareOrdinal(first.RelativePath, second.RelativePath));

        foreach (var file in Files)
        {
            var path = Path.GetFileName(file.RelativePath.Substring(0, file.RelativePath.IndexOf('.')));
            int minId = Int32.Parse(path);
            _minId.Add(minId);
        }

        _modulo = _minId[^1] + 1;
        _lastIndex = 0;
        return Files.Count > 0;
    }
}
