using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Reloaded.Memory;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture.Headers;
using Riders.Tweakbox.Services.Texture.Structs;
using Standart.Hash.xxHash;
using Unsafe = System.Runtime.CompilerServices.Unsafe;
namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// The texture service keeps track of injectable textures sourced from other mods.
/// </summary>
public class TextureService : ISingletonService
{
    // DO NOT CHANGE, TEXTUREDICTIONARY DEPENDS ON THIS.
    private const string HashStringFormat = "X8";

    private List<TextureDictionary> _dictionaries = new List<TextureDictionary>();
    private IModLoader _modLoader;

    public TextureService(IModLoader modLoader)
    {
        _modLoader = modLoader;
        _modLoader.ModLoading += OnModLoading;
        _modLoader.ModUnloading += OnModUnloading;

        // Fill in textures from already loaded mods.
        var existingMods = _modLoader.GetActiveMods().Select(x => x.Generic);
        foreach (var mod in existingMods)
        {
            Add(mod);
        }
    }

    /// <summary>
    /// Gets a hash for a single texture.
    /// </summary>
    /// <param name="textureData">Span which contains only the texture data.</param>
    public string ComputeHashString(Span<byte> textureData)
    {
        textureData = TryFixDdsLength(textureData);

        // Alternative seed for hashing, must not be changed.
        const int AlternateSeed = 1337;

        /*
            According to the birthday attack, you need around 2^(n/2) hashes before a collision.
            As such, using a 32-bit xxHash alone would mean that you only need 2^16 hashes until you
            are to expect a collision which is insufficient.

            Problem is this is a 32-bit process and the 64-bit version of the algorithm is 10x slower;
            so in order to edge out at least a tiny bit more of collision resistance, we hash the file twice
            with a different seed, hoping the combined seed should be more collision resistant.

            This should work as hash functions are designed to have an avalanche effect; changing the seed should
            drastically change the hash and resolve collisions for previously colliding files (I tested this).

            While the world is not ideal and hash functions aren't truly random; probability theory suggests that 
            P(AnB) = P(A) x P(B), which in our case is 2^16 x 2^16, which is 2^32. I highly doubt Riders has 
            anywhere near 4,294,967,296 textures.

            When I tested this with a lowercase English dictionary of 479k words; I went from 14 collisions to 0.
        */

        var xxHashA = xxHash32.ComputeHash(textureData, textureData.Length);
        var xxHashB = xxHash32.ComputeHash(textureData, textureData.Length, AlternateSeed);
        return xxHashA.ToString(HashStringFormat) + xxHashB.ToString(HashStringFormat);
    }

    /// <summary>
    /// Gets the data for a specific texture.
    /// </summary>
    /// <param name="xxHash">Hash of the texture that was loaded.</param>
    /// <param name="data">The loaded texture data.</param>
    /// <param name="info">Info about the returned texture.</param>
    /// <returns>Whether texture data was found.</returns>
    public bool TryGetData(string xxHash, out TextureRef data, out TextureInfo info)
    {
        // Doing this in reverse because mods with highest priority get loaded last.
        // We want to look at those mods first.
        for (int i = _dictionaries.Count - 1; i >= 0; i--)
        {
            if (_dictionaries[i].TryGetTexture(xxHash, out data, out info))
                return true;
        }

        info = default;
        data = default;
        return false;
    }

    private void Add(IModConfigV1 config) => _dictionaries.Add(new TextureDictionary(GetRedirectFolder(config.ModId)));
    private void Remove(IModConfigV1 config)
    {
        var redirectFolder = GetRedirectFolder(config.ModId);
        _dictionaries = _dictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
    private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);
    private string GetRedirectFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/Textures";

    private unsafe Span<byte> TryFixDdsLength(Span<byte> input)
    {
        const int DdsMagic = 0x20534444; // 'DDS '

        /*
            The developers of Sonic Riders pass the right length of data for certain formats (e.g. DXT1).
            This is because they incorrectly assumed DXT1 and others use 1 byte per pixel and incorrectly 
            generate the header. 

            such as D3DXCreateTextureFromFileInMemoryEx; with the length sometimes exceeding the actual size
            of the DDS file.

            We try to detect if the file is a DDS and compute the real length of the DDS file here.
        */

        Struct.FromArray(input, out int magic);
        if (magic != DdsMagic)
            return input;

        // Read Header
        ref var pinned = ref input.GetPinnableReference();
        var ptr = (byte*)Unsafe.AsPointer(ref pinned);
        var ddsHeader = (DdsHeader*)ptr;

        uint headerSize = ddsHeader->DwSize + sizeof(int);
        uint textureSize = 0;

        // Check for uncompressed texture, size is dwPitchOrLinearSize x dwHeight
        // Note: We ignore the mipmap chain because:
        // = I'm lazy to write the code for that.
        // = The game doesn't seem to use them; just creates new ones anyway.
        // = Highly doubt there will be duplicates with different mips.
        if (ddsHeader->DwFlags.HasAllFlags(DdsFlags.Pitch) &&
            ddsHeader->DdsPf.DwFlags.HasAllFlags(DdsPixelFormatFlags.Rgb))
        {
            // Because Riders can't calculate DwPitchOrLinearSize properly, we'll do it ourselves.
            uint pitch = (ddsHeader->DwWidth * ddsHeader->DdsPf.DwRgbBitCount + 7) / 8;
            textureSize = pitch * ddsHeader->DwHeight;
        }
        else if (ddsHeader->DwFlags.HasAllFlags(DdsFlags.LinearSize) &&
                 ddsHeader->DdsPf.DwFlags.HasAllFlags(DdsPixelFormatFlags.Fourcc))
        {
            // 1 = 1 byte per pixel (DXT5)
            // 2 = 0.5 bytes per pixel (DXT1)
            int oneOverBytesPerPixel = ddsHeader->DdsPf.DwFourCc switch
            {
                DdsFourCC.DXT1 => 2,
                DdsFourCC.DXT2 => 1,
                DdsFourCC.DXT3 => 1,
                DdsFourCC.DXT4 => 1,
                DdsFourCC.DXT5 => 1,
                _ => throw new ArgumentOutOfRangeException()
            };

            textureSize = (uint)((ddsHeader->DwWidth * ddsHeader->DwHeight) / oneOverBytesPerPixel);
        }

        return input.Slice(0, (int)(textureSize + headerSize));
    }
}
