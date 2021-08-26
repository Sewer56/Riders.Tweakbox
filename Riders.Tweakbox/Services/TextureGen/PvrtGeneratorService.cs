using Reloaded.Memory;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Services.Texture.Headers;
using Riders.Tweakbox.Services.TextureGen.Structs;
using System;
using Sewer56.SonicRiders.Parser.Texture.Structs;

namespace Riders.Tweakbox.Services.TextureGen;

/// <summary>
/// Service that can be used to generate unique dummy PVRT textures based on DXT1 compression.
/// </summary>
public class PvrtGeneratorService : ISingletonService
{
    /// <summary>
    /// The current shared seed.
    /// This is equal to number of total textures generated.
    /// </summary>
    public ulong Seed { get; private set; }

    /// <summary>
    /// Size of the header in generated texture data.
    /// </summary>
    public const int TextureHeaderSize = 0x40;

    private TextureService _textureService = IoC.GetSingleton<TextureService>();

    /// <summary>
    /// Generates multiple PVRT texture and increments the current seed counter.
    /// </summary>
    /// <param name="options">Texture generation options.</param>
    /// <param name="seed">The unique seed of the first dummy texture generated.</param>
    /// <param name="count">Number of textures to generate.</param>
    /// <returns>An array of PVRT textures, each 72 bytes total, including headers.</returns>
    public byte[][] GenerateMany(int count, PvrtGeneratorSettings options, out ulong seed)
    {
        var result = GC.AllocateUninitializedArray<byte[]>(count);
        seed = Seed;

        for (int x = 0; x < count; x++)
            result[x] = Generate(options, out _);

        return result;
    }

    /// <summary>
    /// Generates a PVRT texture and increments the current seed counter.
    /// </summary>
    /// <param name="seed">The unique seed of the dummy texture generated.</param>
    /// <returns>A dummy PVRT texture, 72 bytes total, including headers.</returns>
    public byte[] Generate(PvrtGeneratorSettings options, out ulong seed)
    {
        var data = GetData(options, Seed);
        seed = Seed++;
        return data;
    }

    /// <summary>
    /// Generates a PVRT texture with a given seed.
    /// </summary>
    /// <param name="seed">The seed.</param>
    /// <returns>A dummy PVRT texture, 72 bytes total, including headers.</returns>
    public unsafe byte[] GetData(PvrtGeneratorSettings options, ulong seed)
    {
        // Alloc
        var fileSize   = _template.Length + _textureService.GetTextureSizeDxt((uint)options.Width, (uint)options.Height, DdsFourCC.DXT1);
        var result     = new byte[fileSize];
        var resultSpan = result.AsSpan();
        
        // Copy
        _template.AsSpan(0, _template.Length).CopyTo(resultSpan);

        // Copy Seed
        Span<byte> seedBytes = stackalloc byte[sizeof(long)];
        Struct.GetBytes(seed, seedBytes);
        seedBytes.CopyTo(resultSpan.Slice(TextureHeaderSize));

        // Fix header
        fixed (byte* data = &result[0])
        {
            var header = (TextureHeader*) data;
            header->Pvrt.Width = options.Width;
            header->Pvrt.Height = options.Height;
            header->Pvrt.TextureSize = (int)(fileSize - 24); // 24 is offset of field
        }

        return result;
    }

    #region
    /* PVRT Texture with: 
     * Pixel Format (RGB565)
     * Resolution: 2x2
     * Size: 8 bytes
     */
    private static byte[] _template = new byte[]
    {
        0x47, 0x42, 0x49, 0x58, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x50, 0x56, 0x52, 0x54, 0x30, 0x00, 0x00, 0x00, 0x06, 0x73, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00,

        // Replaced with actual data (e.g. DDS pointer) only at runtime
        0x01, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x0F, 0x91, 0x08,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
    };
    #endregion
}
