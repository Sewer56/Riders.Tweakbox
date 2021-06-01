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

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Services.Texture.Enums;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX;
using Void = Reloaded.Hooks.Definitions.Structs.Void;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class TextureInjectionController : IController
    {
        private TextureService _textureService = IoC.GetSingleton<TextureService>();
        private AnimatedTextureService _animatedTextureService = IoC.GetSingleton<AnimatedTextureService>();
        private TextureInjectionConfig _config = IoC.GetSingleton<TextureInjectionConfig>();
        
        private static TextureInjectionController _controller;

        private IO _io;
        private IHook<D3DXCreateTextureFromFileInMemoryExPtr> _createTextureHook;
        private IHook<SetTexturePtr> _setTextureHook;
        private IHook<ComReleasePtr> _releaseTextureHook;

        public TextureInjectionController(IO io, IReloadedHooks hooks)
        {
            _io = io;
            _controller = this;
            var d3dx9Handle  = PInvoke.LoadLibrary("d3dx9_25.dll");
            var createTextureFromFileInMemoryEx = Native.GetProcAddress(d3dx9Handle, "D3DXCreateTextureFromFileInMemoryEx");
            _createTextureHook = hooks.CreateHook<D3DXCreateTextureFromFileInMemoryExPtr>(typeof(TextureInjectionController), nameof(CreateTextureFromFileInMemoryHook), (long) createTextureFromFileInMemoryEx).Activate();

            var dx9Hook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value;
            var releaseTextureRefAddress = (long) dx9Hook.Texture9VTable[(int) IDirect3DTexture9.Release].FunctionPointer;
            var setTexturePtrAddress     = dx9Hook.DeviceVTable[(int) IDirect3DDevice9.SetTexture].FunctionPointer;
            _releaseTextureHook = hooks.CreateHook<ComReleasePtr>(typeof(TextureInjectionController), nameof(TextureRelease), releaseTextureRefAddress).Activate();
            _setTextureHook     = hooks.CreateHook<SetTexturePtr>(typeof(TextureInjectionController), nameof(SetTextureHook), (long) setTexturePtrAddress).Activate();
        }

        /// <summary>
        /// Removes duplicates in Auto Common folder.
        /// </summary>
        public void RemoveDuplicatesInCommon()
        {
            var files      = Directory.GetFiles(_io.TextureDumpCommonFolder, TextureCommon.PngFilter, SearchOption.AllDirectories);
            var dictionary = BuildDumpFolderFileNameDictionary();
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (dictionary.ContainsKey(fileName))
                {
                    Log.WriteLine($"Removing Duplicate in Auto Common: {fileName}", LogCategory.TextureDump);
                    File.Delete(file);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe int CreateTextureFromFileInMemoryHookInstance(byte* deviceref, byte* srcdataref, int srcdatasize, int width, int height, int miplevels, Usage usage, Format format, Pool pool, int filter, int mipfilter, RawColorBGRA colorkey, byte* srcinforef, PaletteEntry* paletteref, byte** textureout)
        {
            // If not enabled, don't do anything.
            if (!_config.Data.LoadTextures && !_config.Data.DumpTextures)
                return _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));
            
            // Hash the texture,
            var xxHash = _textureService.ComputeHashString(new Span<byte>(srcdataref, srcdatasize));

            // Load alternative texture if necessary.
            if (_config.Data.LoadTextures && _textureService.TryGetData(xxHash, out var data, out var info))
            {
                using var textureRef = data;
                Log.WriteLine($"Loading Custom Texture: {info.Path}", LogCategory.TextureLoad);
                fixed (byte* dataPtr = &data.Data[0])
                {
                    var texture = _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, dataPtr, textureRef.Data.Length, 0, 0, 0, usage, Format.Unknown, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));
                    if (info.Type == TextureType.Animated)
                        _animatedTextureService.TrackAnimatedTexture(*textureout, info.Animated);

                    return texture;
                }
            }

            var result = _createTextureHook.OriginalFunction.Ptr.Invoke(deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, PointerExtensions.ToBlittable(textureout));
            
            // Dump texture if successfully loaded.
            if (result == Result.Ok && _config.Data.DumpTextures)
                DumpTexture(new Texture((IntPtr) (*textureout)), xxHash);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private IntPtr SetTextureHookInstance(IntPtr devicepointer, int stage, void* texture)
        {
            if (_animatedTextureService.TryGetAnimatedTexture(texture, *State.TotalFrameCounter, out var newTexture))
                return _setTextureHook.OriginalFunction.Value.Invoke(devicepointer, stage, (Void*) newTexture);

            return _setTextureHook.OriginalFunction.Value.Invoke(devicepointer, stage, (Void*) texture);
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
                _animatedTextureService.ReleaseAnimatedTexture((void*) thisptr);

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
                        Log.WriteLine($"Skipped Texture [Only New]: {fileName}", LogCategory.TextureDump);
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
                            Log.WriteLine($"Deduplicating Texture [{entry.NumAppearances} Duplicates]: {fileName}", LogCategory.TextureDump);
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
                    Log.WriteLine($"Dumped Texture: {fileName}", LogCategory.TextureDump);
                    break;
            }
        }

        private Dictionary<string, TextureDictEntry> BuildDumpFolderFileNameDictionary()
        {
            var result   = new Dictionary<string, TextureDictEntry>();
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

#if DEBUG
        [UnmanagedCallersOnly]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static IntPtr SetTextureHook(IntPtr devicepointer, int stage, void* texture) => _controller.SetTextureHookInstance(devicepointer, stage, texture);

#if DEBUG
        [UnmanagedCallersOnly]
#endif
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

        [Function(CallingConventions.Stdcall)]
        public struct D3DXCreateTextureFromFileInMemoryExPtr
        {
            public FuncPtr<BlittablePointer<byte>, BlittablePointer<byte>, int, int, int, int, Usage, Format, Pool, int, int, 
                           RawColorBGRA, BlittablePointer<byte>, BlittablePointer<PaletteEntry>, BlittablePointer<BlittablePointer<byte>>, int> Ptr;
        }

#if !DEBUG
        [ManagedFunction(CallingConventions.ClrCall)]
#endif
        [Function(CallingConventions.Stdcall)]
        public struct ComReleasePtr { public FuncPtr<IntPtr, IntPtr> Value; }

#if !DEBUG
        [ManagedFunction(CallingConventions.ClrCall)]
#endif
        [Function(CallingConventions.Stdcall)]
        public struct SetTexturePtr { public FuncPtr<IntPtr, int, BlittablePointer<Void>, IntPtr> Value; }
    }
}
