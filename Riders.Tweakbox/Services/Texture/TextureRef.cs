using System.Buffers;
using System.IO;
using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// References texture data.
/// </summary>
public ref struct TextureRef
{
    public byte[] Data;
    public ArrayPool<byte> Owner;
    public bool NeedsDispose;

    public TextureFormat Format;
    public bool IsCached;

    /// <inheritdoc />
    public TextureRef(byte[] data, TextureFormat format, bool isCached = false) : this()
    {
        Data = data;
        Format = format;
        IsCached = isCached;
    }

    public void Dispose()
    {
        if (NeedsDispose)
            Owner.Return(Data);
    }

    /// <summary>
    /// Returns true if a texture should be cached, else false.
    /// </summary>
    public bool ShouldBeCached() => Format.ShouldBeCached() && !IsCached;

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

        return type == TextureFormat.DdsLz4 ? TextureCompression.PickleFromFileToRef(filePath) : new TextureRef(File.ReadAllBytes(filePath), type);
    }
}
