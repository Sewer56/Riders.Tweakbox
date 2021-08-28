using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Services.Texture.Enums;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX;
using Void = Reloaded.Hooks.Definitions.Structs.Void;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Texture.Structs;

namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Note: This is implementation only.
/// Please see <see cref="TextureService"/> for utility methods to influence the injector behaviour.
/// </summary>
public unsafe class TextureInjectionController : IController
{
    private const int D3DX_FROM_FILE = -3;
    private TextureService _textureService = IoC.GetSingleton<TextureService>();
    private TextureCacheService _cacheService = IoC.GetSingleton<TextureCacheService>();
    private AnimatedTextureService _animatedTextureService = IoC.GetSingleton<AnimatedTextureService>();
    private TextureInjectionConfig _config = IoC.GetSingleton<TextureInjectionConfig>();

    private static TextureInjectionController _controller;

    private IO _io;
    private IHook<D3DFunctions.D3DXCreateTextureFromFileInMemoryExPtr> _createTextureHook;
    private IHook<D3DFunctions.SetTexturePtr> _setTextureHook;
    private IHook<D3DFunctions.ComReleasePtr> _releaseTextureHook;
    private Direct3DController _d3dController = IoC.GetSingleton<Direct3DController>();

    private Logger _logDump = new Logger(LogCategory.TextureDump);
    private Logger _logLoad = new Logger(LogCategory.TextureLoad);

    public TextureInjectionController(IO io, IReloadedHooks hooks)
    {
        _io = io;
        _controller = this;

        _createTextureHook = D3DFunctions.CreateTexture.Hook(typeof(TextureInjectionController), nameof(CreateTextureFromFileInMemoryHook)).Activate();
        _releaseTextureHook = D3DFunctions.ReleaseTexture.Hook(typeof(TextureInjectionController), nameof(TextureRelease)).Activate();
        _setTextureHook = D3DFunctions.SetTexture.Hook(typeof(TextureInjectionController), nameof(SetTextureHook)).Activate();
    }

    /// <summary>
    /// Removes duplicates in Auto Common folder.
    /// </summary>
    public void RemoveDuplicatesInCommon()
    {
        var files = Directory.GetFiles(_io.TextureDumpCommonFolder, TextureCommon.PngFilter, SearchOption.AllDirectories);
        var dictionary = BuildDumpFolderFileNameDictionary();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (dictionary.ContainsKey(fileName))
            {
                _logDump.WriteLine($"Removing Duplicate in Auto Common: {fileName}");
                File.Delete(file);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private unsafe int CreateTextureFromFileInMemoryHookInstance(byte* deviceref, byte* srcdataref, int srcdatasize, int width, int height, int miplevels, Usage usage, Format format, Pool pool, int filter, int mipfilter, RawColorBGRA colorkey, byte* srcinforef, PaletteEntry* paletteref, byte** textureout)
    {
        // If not enabled, don't do anything.
        if (!_d3dController.IsRidersDevice((IntPtr)deviceref) || (!_config.Data.LoadTextures && !_config.Data.DumpTextures))
            return _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));

        // Hash the texture,
        int result = 0;
        var xxHash = _textureService.ComputeHashString(new Span<byte>(srcdataref, srcdatasize));
        miplevels  = _textureService.ShouldGenerateMipmap(xxHash) ? miplevels : D3DX_FROM_FILE;
        
        // Load alternative texture if necessary.
        if (_config.Data.LoadTextures && _textureService.TryGetData(xxHash, out var data, out var info))
            return LoadCustomTexture(xxHash, info, data, deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, textureout);

        result = _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));
        if (result == 0)
            _textureService.AddD3dTexture(new TextureCreationParameters(xxHash, (IntPtr)deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, textureout, false));

        // Dump texture if successfully loaded.
        if (result == Result.Ok && _config.Data.DumpTextures)
            DumpTexture(new Texture((IntPtr)(*textureout)), xxHash);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal unsafe int LoadCustomTexture(string xxHash, TextureInfo info, TextureRef data, byte* deviceref, byte* srcdataref, int srcdatasize, int width, int height, int miplevels, Usage usage, Format format, Pool pool, int filter, int mipfilter, RawColorBGRA colorkey, byte* srcinforef, PaletteEntry* paletteref, byte** textureout)
    {
        _logLoad.WriteLine($"Loading Custom Texture: [{xxHash}] {info.Path}");
        fixed (byte* dataPtr = &data.Data[0])
        {
            int result = _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, dataPtr, data.Data.Length, 0, 0, 0, usage, Format.Unknown, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));
            if (data.ShouldBeCached())
                _cacheService.QueueStore(info.Path, new Texture((IntPtr)(*textureout)));

            if (info.Type == TextureType.Animated)
                _animatedTextureService.TrackAnimatedTexture(*textureout, info.Animated);

            // Note: Adding original texture details so we can re-replace the texture here when reloading.
            if (result == 0)
                _textureService.AddD3dTexture(new TextureCreationParameters(xxHash, (IntPtr)deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, textureout, true));

            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IntPtr SetTextureHookInstance(IntPtr devicepointer, int stage, void* texture)
    {
        if (!_d3dController.IsRidersDevice(devicepointer) || 
            !_animatedTextureService.TryGetAnimatedTexture(texture, *State.TotalFrameCounter, out var newTexture))
            return _setTextureHook.OriginalFunction.Value.Invoke(devicepointer, stage, (Void*)texture);

        return _setTextureHook.OriginalFunction.Value.Invoke(devicepointer, stage, (Void*)newTexture);
    }

    private bool _isReleasing;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IntPtr ReleaseTexture(IntPtr thisptr)
    {
        if (_isReleasing)
            return _releaseTextureHook.OriginalFunction.Value.Invoke(thisptr);

        _isReleasing = true;
        var count = _releaseTextureHook.OriginalFunction.Value.Invoke(thisptr);
        if ((int)count == 0)
        {
            _animatedTextureService.ReleaseAnimatedTexture((void*)thisptr);
            _textureService.RemoveD3dTexture(thisptr);
        }

        _isReleasing = false;
        return count;
    }

    private void DumpTexture(Texture texture, string xxHash)
    {
        // Hash The Texture
        var description = texture.GetLevelDescription(0);
        var fileName = $"{description.Width}x{description.Height}_{xxHash}.png"; // DO NOT PREPEND, ONLY APPEND

        switch (_config.Data.DumpingMode)
        {
            case TextureInjectionConfig.DumpingMode.OnlyNew:
            {
                var dictionary = BuildDumpFolderFileNameDictionary();
                if (dictionary.ContainsKey(fileName))
                {
                    _logDump.WriteLine($"Skipped Texture [Only New]: {fileName}");
                    return;
                }

                goto case TextureInjectionConfig.DumpingMode.All;
            }

            case TextureInjectionConfig.DumpingMode.Deduplicate:
            {
                var dictionary = BuildDumpFolderFileNameDictionary();
                if (dictionary.ContainsKey(fileName))
                {
                    var entry = dictionary[fileName];
                    if (entry.NumAppearances > _config.Data.DeduplicationMaxFiles)
                    {
                        _logDump.WriteLine($"Deduplicating Texture [{entry.NumAppearances} Duplicates]: {fileName}");
                        File.Move(entry.FullPaths[0], Path.Combine(_io.TextureDumpCommonFolder, fileName), true);
                        for (int x = 1; x < entry.FullPaths.Count; x++)
                            File.Delete(entry.FullPaths[x]);
                        
                        return;
                    }
                }

                // Ensure file isn't in common already.
                var commonDictionary = Directory.GetFiles(_io.TextureDumpCommonFolder, TextureCommon.PngFilter, SearchOption.AllDirectories)
                                                .Select(Path.GetFileName).ToHashSet();
                 
                if (commonDictionary.Contains(fileName))
                    return;

                goto case TextureInjectionConfig.DumpingMode.All;
            }

            case TextureInjectionConfig.DumpingMode.All:
                BaseTexture.ToFile(texture, Path.Combine(_io.TextureDumpFolder, fileName), ImageFileFormat.Png);
                _logDump.WriteLine($"Dumped Texture: {fileName}");
                break;
        }
    }

    private Dictionary<string, TextureDictEntry> BuildDumpFolderFileNameDictionary()
    {
        var result = new Dictionary<string, TextureDictEntry>();
        var allFiles = Directory.GetFiles(_io.TextureDumpFolder, TextureCommon.PngFilter, SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            // Ignore Common Folder
            if (file.StartsWith(_io.TextureDumpCommonFolder))
                continue;

            var fileName = Path.GetFileName(file);
            if (!result.ContainsKey(fileName))
                result[fileName] = new TextureDictEntry() { FullPaths = new List<string>(), NumAppearances = 0 };

            var entry = result[fileName];
            entry.NumAppearances += 1;
            entry.FullPaths.Add(file);
            result[fileName] = entry;
        }

        return result;
    }

    protected struct TextureDictEntry
    {
        public int NumAppearances;
        public List<string> FullPaths;
    }

    [UnmanagedCallersOnly]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IntPtr SetTextureHook(IntPtr devicepointer, int stage, void* texture) => _controller.SetTextureHookInstance(devicepointer, stage, texture);

    [UnmanagedCallersOnly]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IntPtr TextureRelease(IntPtr texturePtr) => _controller.ReleaseTexture(texturePtr);

    [UnmanagedCallersOnly]
    private static unsafe int CreateTextureFromFileInMemoryHook(byte* deviceref, byte* srcdataref, int srcdatasize,
        int width, int height, int miplevels, Usage usage, Format format, Pool pool, int filter, int mipfilter,
        RawColorBGRA colorkey, byte* srcinforef, PaletteEntry* paletteref, byte** textureout)
    {
        return _controller.CreateTextureFromFileInMemoryHookInstance(deviceref, srcdataref, srcdatasize, width,
            height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref,
            textureout);
    }
}
