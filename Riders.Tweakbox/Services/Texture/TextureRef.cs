﻿using System.IO;
using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// References texture data.
/// </summary>
public struct TextureRef
{
    public byte[] Data;
    public TextureFormat Format;
    public bool IsCached;

    /// <inheritdoc />
    public TextureRef(byte[] data, TextureFormat format, bool isCached = false) : this()
    {
        Data = data;
        Format = format;
        IsCached = isCached;
    }
    
    /// <summary>
    /// Returns true if a texture should be cached, else false.
    /// </summary>
    public bool ShouldBeCached() => Format.ShouldBeCached() && !IsCached;

    /// <summary>
    /// Gets a texture reference from file; ignoring the cache..
    /// </summary>
    /// <param name="filePath">The file path to the texture.</param>
    /// <param name="type">The texture type.</param>
    public static TextureRef FromFileUncached(string filePath, TextureFormat type) => type == TextureFormat.DdsLz4 ? TextureCompression.PickleFromFileToRef(filePath) : new TextureRef(File.ReadAllBytes(filePath), type);

    /// <summary>
    /// Gets a texture reference from file.
    /// </summary>
    /// <param name="filePath">The file path to the texture.</param>
    /// <param name="type">The texture type.</param>
    public static TextureRef FromFile(string filePath, TextureFormat type)
    {
        var cache = TextureCacheService.Instance;
        if (type.ShouldBeCached() && cache != null && cache.TryFetch(filePath, out var result))
            return result;

        return FromFileUncached(filePath, type);
    }
}
