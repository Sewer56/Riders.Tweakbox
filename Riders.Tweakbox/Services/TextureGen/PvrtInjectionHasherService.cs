using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture;
using System;
using System.Collections.Generic;
namespace Riders.Tweakbox.Services.TextureGen;

/// <summary>
/// Service which converts dummy PVRT textures into DDS using game code and hashes them.
/// This allows for runtime replacement of dummy textures using <see cref="Texture.TextureService"/>.
/// </summary>
public class PvrtInjectionHasherService : ISingletonService
{
    private TextureService _textureService = IoC.GetSingleton<TextureService>();
    private PvrtConverterService _converterService = IoC.GetSingleton<PvrtConverterService>();

    /// <summary>
    /// Converts a texture to DDS and hashes it.
    /// </summary>
    /// <param name="pvrtData">An array of PVRT textures to convert and hash.</param>
    /// <returns></returns>
    public string[] HashPvrtMany(byte[][] pvrtData)
    {
        var hashes = new string[pvrtData.Length];

        for (int x = 0; x < pvrtData.Length; x++)
            hashes[x] = HashPvrt(pvrtData[x]);

        return hashes;
    }

    /// <summary>
    /// Converts a texture to DDS and hashes it.
    /// </summary>
    /// <param name="pvrtData">The PVRT texture to convert and hash.</param>
    public string HashPvrt(Span<byte> pvrtData)
    {
        var ddsData = _converterService.Convert(pvrtData);
        return HashDds(ddsData);
    }

    /// <summary>
    /// Hashes a DDS texture, returning the same hash as needed for <see cref="TextureService"/>.
    /// </summary>
    /// <param name="ddsData">The DDS texture to hash.</param>
    public string HashDds(Span<byte> ddsData)
    {
        return _textureService.ComputeHashString(ddsData);
    }
}
