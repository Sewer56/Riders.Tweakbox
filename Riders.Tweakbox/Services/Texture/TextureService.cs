using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EnumsNET;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Services.Texture.Headers;
using Riders.Tweakbox.Services.Texture.Interfaces;
using Riders.Tweakbox.Services.Texture.Structs;
using SharpDX.Direct3D9;
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
    private const int DdsMagic = 0x20534444; // 'DDS '
    private static int BiggestTextureSizeEncountered = 0x20000;

    private List<ManualTextureDictionary> _fallbackDictionaries = new List<ManualTextureDictionary>();
    private List<AutoTextureDictionary> _autoDictionaries       = new List<AutoTextureDictionary>();
    private List<ManualTextureDictionary> _priorityDictionaries = new List<ManualTextureDictionary>();

    private IModLoader _modLoader;
    private HashSet<string> _ignoreMipmapHashes = new HashSet<string>();

    // Internal Overrides
    private string _forcedTextureHash;
    private unsafe byte* _forcedHashAddress;

    // Tracking all textures
    private Dictionary<string, TextureCreationParameters> _texturesByHash    = new Dictionary<string, TextureCreationParameters>();
    private Dictionary<IntPtr, TextureCreationParameters> _texturesByPointer = new Dictionary<IntPtr, TextureCreationParameters>();
    private Logger _logLoad = new Logger(LogCategory.TextureLoad);
    private Logger _logDebug = new Logger(LogCategory.TextureDebug);

    // Controller Related
    private bool _areControllersSetup;
    private TextureInjectionController _injectionController;
    private ConcurrentQueue<TextureReloadParameters> _reloadTextureQueue = new ConcurrentQueue<TextureReloadParameters>();
    
    public TextureService(IModLoader modLoader)
    {
        _modLoader = modLoader;
        _modLoader.ModLoading += OnModLoading;
        _modLoader.ModUnloading += OnModUnloading;

        // Fill in textures from already loaded mods.
        var existingMods = _modLoader.GetActiveMods().Select(x => x.Generic);
        foreach (var mod in existingMods)
            Add(mod);
    }

    /// <summary>
    /// Gets a hash for a single texture.
    /// </summary>
    /// <param name="textureData">Span which contains only the texture data.</param>
    public unsafe string ComputeHashString(Span<byte> textureData)
    {
        // Use forced hash override if required.
        if (TryGetForcedTextureHash(textureData, out var hash)) 
            return hash;

        // Else hash the texture data.
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
        if (TryGetDataFromDictionary(_priorityDictionaries, xxHash, out data, out info))
            return true;

        if (TryGetDataFromDictionary(_autoDictionaries, xxHash, out data, out info))
            return true;

        if (TryGetDataFromDictionary(_fallbackDictionaries, xxHash, out data, out info))
            return true;

        info = default;
        data = default;
        return false;
    }

    /// <summary>
    /// Gets information about a specific texture redirection.
    /// </summary>
    /// <param name="xxHash">Hash of the texture that was loaded.</param>
    /// <param name="info">Info about the texture redirection.</param>
    /// <returns>Whether texture redirect info was found.</returns>
    public bool TryGetInfo(string xxHash, out TextureInfo info)
    {
        // Doing this in reverse because mods with highest priority get loaded last.
        // We want to look at those mods first.
        if (TryGetInfoFromDictionary(_priorityDictionaries, xxHash, out info))
            return true;

        if (TryGetInfoFromDictionary(_autoDictionaries, xxHash, out info))
            return true;

        if (TryGetInfoFromDictionary(_fallbackDictionaries, xxHash, out info))
            return true;

        info = default;
        return false;
    }

    /// <summary>
    /// Adds a texture dictionary that you can use for redirecting in-game textures.
    /// Once you no longer want the dictionary used, use <see cref="RemoveDictionary"/>.
    /// </summary>
    /// <param name="priority">
    ///     If true, this dictionary should take priority over textures.
    ///     Automatically imported without code.
    ///     If false, will be a fallback when no other texture is available.
    /// </param>
    public void AddDictionary(ManualTextureDictionary dict, bool priority = true)
    {
        if (priority)
            _priorityDictionaries.Add(dict);
        else
            _fallbackDictionaries.Add(dict);
    }

    /// <summary>
    /// Removes a dictionary from being used.
    /// </summary>
    /// <param name="dict">The dictionary.</param>
    public void RemoveDictionary(ManualTextureDictionary dict)
    {
        _priorityDictionaries.Remove(dict);
        _fallbackDictionaries.Remove(dict);
    }

    /// <summary>
    /// Ensures the game does not generate a mipmap chain for a DDS
    /// texture with a given hash.
    /// </summary>
    /// <param name="xxHash">Hash of the DDS.</param>
    public void DontGenerateMipmaps(string xxHash) => _ignoreMipmapHashes.Add(xxHash);

    /// <summary>
    /// Stops ignoring a certain hash and allows it to create mipmaps on future loads.
    /// </summary>
    /// <param name="xxHash">Hash of the DDS.</param>
    public void GenerateMipmaps(string xxHash) => _ignoreMipmapHashes.Remove(xxHash);

    /// <summary>
    /// Returns true if a mipmap should be generated for a given hash.
    /// </summary>
    /// <returns></returns>
    public bool ShouldGenerateMipmap(string xxHash) => !_ignoreMipmapHashes.Contains(xxHash);

    /// <summary>
    /// Calculates the size of an individual texture or mipmap in a mipmap chain.
    /// </summary>
    /// <param name="fourCc">The DXT format used.</param>
    /// <returns></returns>
    public uint GetTextureSizeDxt(uint width, uint height, DdsFourCC fourCc)
    {
        // 1 = 1 byte per pixel (DXT5)
        // 2 = 0.5 bytes per pixel (DXT1)
        int oneOverBytesPerPixel = fourCc switch
        {
            DdsFourCC.DXT1 => 2,
            DdsFourCC.DXT2 => 1,
            DdsFourCC.DXT3 => 1,
            DdsFourCC.DXT4 => 1,
            DdsFourCC.DXT5 => 1,
            _ => throw new ArgumentOutOfRangeException()
        };

        return (uint)((width * height) / oneOverBytesPerPixel);
    }

    /// <summary>
    /// Calculates the size of an individual texture or mipmap in a mipmap chain.
    /// </summary>
    /// <param name="rgbBitCount">Number of bits in an RGB (possibly including alpha) format.</param>
    /// <returns></returns>
    public uint GetTextureSizePixel(uint width, uint height, uint rgbBitCount)
    {
        uint pitch = (width * rgbBitCount + 7) / 8;
        return pitch * height;
    }

    /// <summary>
    /// Tries to get a live Direct3D texture using an xxHash of the texture.
    /// </summary>
    /// <param name="hash">The hash of the textures.</param>
    /// <param name="creation">The parameters with which the texture was created.</param>
    /// <returns>True if found, else false.</returns>
    public bool TryGetD3dTexture(string hash, out TextureCreationParameters creation) => _texturesByHash.TryGetValue(hash, out creation);

    /// <summary>
    /// Tries to get a live Direct3D texture using the native pointer of the texture.
    /// </summary>
    /// <param name="thisPtr">Pointer of the texture.</param>
    /// <param name="creation">The parameters with which the texture was created.</param>
    /// <returns>True if found, else false.</returns>
    public bool TryGetD3dTexture(IntPtr thisPtr, out TextureCreationParameters creation) => _texturesByPointer.TryGetValue(thisPtr, out creation);

    /// <summary>
    /// Retrieves all d3d textures.
    /// </summary>
    public TextureCreationParameters[] GetAllD3dTextures() => _texturesByPointer.Values.ToArray();

    /// <summary>
    /// Tries to force reload a custom texture.
    /// Note: Force reloading the wrong texture may result in a crash.
    /// </summary>
    /// <param name="hash">The hash.</param>
    /// <returns>True on success, else false.</returns>
    internal unsafe bool TryReloadCustomTexture(string hash)
    {
        // Setup force reload if not ready.
        EnsurePacingControllerSetup();
        bool hasData    = TryGetData(hash, out var textureRef, out var textureInfo);
        bool hasTexture = TryGetD3dTexture(hash, out var texture);
        if (!hasTexture || !hasData)
        {
            _logLoad.WriteLine($"Reload of Custom Texture {hash} was requested but texture is not loaded or replacement was not found.");
            return false;
        }

        _reloadTextureQueue.Enqueue(new TextureReloadParameters(textureRef, textureInfo, texture));
        return true;
    }

    /// <summary>
    /// Tries to force reload a texture.
    /// </summary>
    /// <param name="hash">The hash.</param>
    /// <returns>True on success, else false.</returns>
    internal unsafe bool TryReloadTexture(string hash)
    {
        // Setup force reload if not ready.
        EnsurePacingControllerSetup();
        bool hasTexture = TryGetD3dTexture(hash, out var texture);
        if (!hasTexture)
        {
            _logLoad.WriteLine($"Reload of Texture {hash} was requested but texture is not loaded.");
            return false;
        }

        _reloadTextureQueue.Enqueue(new TextureReloadParameters(texture));
        return true;
    }

    /// <summary>
    /// Adds an individual D3d Texture.
    /// </summary>
    internal void AddD3dTexture(TextureCreationParameters parameters)
    {
        _logDebug.WriteLine($"[{nameof(AddD3dTexture)}] Adding Texture {(long)parameters.NativePointer:X}");
        _texturesByPointer[parameters.NativePointer] = parameters;
        _texturesByHash[parameters.Hash] = parameters;
    }

    /// <summary>
    /// Adds an individual D3d Texture.
    /// </summary>
    internal void RemoveD3dTexture(IntPtr thisPtr)
    {
        if (TryGetD3dTexture(thisPtr, out var parameters))
        {
            _texturesByPointer.Remove(thisPtr);
            _texturesByHash.Remove(parameters.Hash);
            _logDebug.WriteLine($"[{nameof(RemoveD3dTexture)}] Removed Texture {(long)thisPtr:X}");
        }
        else
        {
            _logDebug.WriteLine($"[{nameof(RemoveD3dTexture)}] Texture {(long)thisPtr:X} NOT FOUND");
        }
    }

    /// <summary>
    /// Retrieves a list of all texture dictionaries.
    /// </summary>
    /// <param name="result">The list of dictionaries to receive the results.</param>
    internal void GetTextureDictionaries(List<TextureDictionaryBase> result)
    {
        result.AddRange(_autoDictionaries);
        result.AddRange(_fallbackDictionaries);
        result.AddRange(_priorityDictionaries);
    }

    // Reload texture and/or custom texture after frame.
    private unsafe void AfterD3dEndFrame()
    {
        IntPtr buffer = IntPtr.Zero;

        var releaseTexture = D3DFunctions.ReleaseTexture.GetWrapper().Value;
        var createTexture = D3DFunctions.CreateTexture.GetWrapper();

        while (_reloadTextureQueue.TryDequeue(out var reloadTexture))
        {
            var parameters = reloadTexture.Parameters;
            _logLoad.WriteLine($"Reloading Texture [{parameters.Hash}], Pointer {(long)parameters.NativePointer:X}, ppTexture: {(long)parameters.TextureOut:X}");
            
            // Unload 
            if (parameters.IsCustomTexture)
            {
                // Initialize custom texture data if needed.
                if (!reloadTexture.CustomTextureDataInitialized)
                {
                    if (!TryGetData(parameters.Hash, out reloadTexture.Ref, out reloadTexture.Info))
                    {
                        _logLoad.WriteLine($"Not Reloading Custom Texture [{parameters.Hash}]. Data was Not Found!!");
                        continue;
                    }
                }

                int count = (int)releaseTexture.Invoke(parameters.NativePointer);
                if (count > 0)
                    _logLoad.WriteLine($"WARNING!! Texture Reload: Custom Texture Reference Count > 0");
                
                _injectionController.LoadCustomTexture(parameters.Hash, reloadTexture.Info, reloadTexture.Ref,
                    (byte*)parameters.Device, parameters.SrcDataRef, parameters.SrcDataSize,
                    parameters.Width, parameters.Height, parameters.MipLevels,
                    parameters.Usage, parameters.Format, parameters.Pool, parameters.Filter,
                    parameters.MipFilter, parameters.ColorKey, parameters.SrcInfoRef, parameters.PaletteRef,
                    parameters.TextureOut);
            }
            else
            {
                if (buffer == IntPtr.Zero)
                    buffer = Marshal.AllocHGlobal(BiggestTextureSizeEncountered);

                try
                {
                    // First dump using D3D since original data source was likely changed, so dump and then load again.
                    var pTexture = (IntPtr)(*parameters.TextureOut);
                    if (pTexture != parameters.NativePointer)
                    {
                        _logLoad.WriteLine($"WARNING!! pTexture does not match NativePointer. Not Reloading.");
                        continue;
                    }

                    var texture = new SharpDX.Direct3D9.Texture(parameters.NativePointer);
                    using var stream = SharpDX.Direct3D9.Texture.ToStream(texture, ImageFileFormat.Dds);
                    int ddsLength = (int)stream.RemainingLength;
                    if (ddsLength > BiggestTextureSizeEncountered)
                    {
                        Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(ddsLength);
                        BiggestTextureSizeEncountered = ddsLength;
                    }

                    stream.Read(new Span<byte>((void*)buffer, BiggestTextureSizeEncountered));
                    int count = (int)releaseTexture.Invoke(parameters.NativePointer);
                    if (count > 0)
                        _logLoad.WriteLine($"WARNING!! Texture Reload: Custom Texture Reference Count > 0");

                    ForceNextTextureHash(parameters.Hash, (byte*) buffer);
                    RemoveD3dTexture(parameters.NativePointer);
                    createTexture.Ptr.Invoke((byte*)parameters.Device, (byte*) buffer, ddsLength,
                        parameters.Width, parameters.Height, parameters.MipLevels,
                        parameters.Usage, parameters.Format, parameters.Pool, parameters.Filter,
                        parameters.MipFilter, parameters.ColorKey, null, null,
                        PointerExtensions.ToBlittable(parameters.TextureOut));
                }
                catch (Exception e)
                {
                    _logLoad.WriteLine($"Failed to Reload Texture [{parameters.Hash}] {e.Message}");
                }
            }
        }

        if (buffer != IntPtr.Zero)
            Marshal.FreeHGlobal(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool TryGetDataFromDictionary<T>(List<T> dictionaries, string xxHash, out TextureRef data, out TextureInfo info) where T : ITextureDictionary
    {
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            if (dictionaries[i].TryGetTexture(xxHash, out data, out info))
                return true;
        }

        data = default;
        info = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool TryGetInfoFromDictionary<T>(List<T> dictionaries, string xxHash, out TextureInfo info) where T : ITextureDictionary
    {
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            if (dictionaries[i].TryGetTextureInfo(xxHash, out info))
                return true;
        }
        
        info = default;
        return false;
    }

    private void Add(IModConfigV1 config) => _autoDictionaries.Add(new AutoTextureDictionary(GetRedirectFolder(config.ModId)));
    private void Remove(IModConfigV1 config)
    {
        var redirectFolder = GetRedirectFolder(config.ModId);
        _autoDictionaries = _autoDictionaries.Where(x => !x.Source.Equals(redirectFolder, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnModUnloading(IModV1 mod, IModConfigV1 config) => Remove(config);
    private void OnModLoading(IModV1 mod, IModConfigV1 config) => Add(config);
    private string GetRedirectFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/Textures";

    private unsafe Span<byte> TryFixDdsLength(Span<byte> input)
    {
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
            textureSize = GetTextureSizePixel(ddsHeader->DwWidth, ddsHeader->DwHeight, ddsHeader->DdsPf.DwRgbBitCount);
        }
        else if (ddsHeader->DwFlags.HasAllFlags(DdsFlags.LinearSize) &&
                 ddsHeader->DdsPf.DwFlags.HasAllFlags(DdsPixelFormatFlags.Fourcc))
        {
            textureSize = GetTextureSizeDxt(ddsHeader->DwWidth, ddsHeader->DwHeight, ddsHeader->DdsPf.DwFourCc);
        }

        return input.Slice(0, (int)(textureSize + headerSize));
    }

    private void EnsurePacingControllerSetup()
    {
        if (_areControllersSetup)
            return;

        var pacingController = IoC.GetSingleton<FramePacingController>();
        pacingController.AfterEndFrame += AfterD3dEndFrame;

        _injectionController = IoC.GetSingleton<TextureInjectionController>();
        _areControllersSetup = true;
    }

    /// <summary>
    /// Forces the next texture hash to have a particular value.
    /// </summary>
    private unsafe void ForceNextTextureHash(string xxHash, byte* address)
    {
        _forcedTextureHash = xxHash;
        _forcedHashAddress = address;
    }

    private unsafe bool TryGetForcedTextureHash(Span<byte> textureData, out string hash)
    {
        hash = default;
        if (_forcedTextureHash != null && Unsafe.AsPointer(ref textureData.GetPinnableReference()) == _forcedHashAddress)
        {
            /*
                What's the deal here?

                Basically, the headers of dumped DDSes are inconsistent
                with the source textures that were loaded using DirectX; 
                which causes issues for texture injection as the resulting 
                hashes would differ.
             
                Making the hashes exclude the header would break existing texture mods;
                so sometimes when force reloading textures we just have to force a certain hash.
            */
            var result = _forcedTextureHash;
            _forcedTextureHash = null;
            hash = result;
            return true;
        }

        return false;
    }
}
