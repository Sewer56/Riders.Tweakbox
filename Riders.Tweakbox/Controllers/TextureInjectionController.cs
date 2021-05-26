using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Services.Texture;
using Sewer56.SonicRiders;
using SharpDX;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class TextureInjectionController : IController
    {
        private TextureService _textureService = IoC.GetSingleton<TextureService>();
        private TextureInjectionConfig _config = IoC.GetSingleton<TextureInjectionConfig>();

        private IO _io;
        private IHook<D3DXCreateTextureFromFileInMemoryEx> _createTextureHook;
        
        public TextureInjectionController(IO io)
        {
            _io = io;
            var d3dx9Handle  = PInvoke.LoadLibrary("d3dx9_25.dll");
            var createTextureFromFileInMemoryEx = Native.GetProcAddress(d3dx9Handle, "D3DXCreateTextureFromFileInMemoryEx");
            _createTextureHook = SDK.ReloadedHooks.CreateHook<D3DXCreateTextureFromFileInMemoryEx>(CreateTextureFromFileInMemoryHook, (long) createTextureFromFileInMemoryEx).Activate();
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

        private unsafe int CreateTextureFromFileInMemoryHook(void* deviceref, void* srcdataref, int srcdatasize, int width, int height, int miplevels, int usage, Format format, Pool pool, int filter, int mipfilter, RawColorBGRA colorkey, void* srcinforef, PaletteEntry* paletteref, void** textureout)
        {
            // Hash the texture,
            var xxHash = _textureService.ComputeHashString(new Span<byte>(srcdataref, srcdatasize));

            // Load alternative texture if necessary.
            if (_config.Data.LoadTextures && _textureService.TryGetData(xxHash, out var data, out var filePath))
            {
                using var textureRef = data;
                Log.WriteLine($"Loading Custom Texture: {filePath}", LogCategory. TextureLoad);
                fixed (byte* dataPtr = &data.Data[0])
                {
                    return _createTextureHook.OriginalFunction(deviceref, dataPtr, textureRef.Data.Length, 0, 0, 0, usage, Format.Unknown, pool, filter, mipfilter, colorkey, srcinforef, paletteref, textureout);
                }
            }

            var result = _createTextureHook.OriginalFunction(deviceref, srcdataref, srcdatasize, width, height, miplevels, usage, format, pool, filter, mipfilter, colorkey, srcinforef, paletteref, textureout);
            
            // Dump texture if successfully loaded.
            if (result == Result.Ok && _config.Data.DumpTextures)
                DumpTexture(new Texture((IntPtr) (*textureout)), xxHash);

            return result;
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

        [Function(CallingConventions.Stdcall)]
        public unsafe delegate int D3DXCreateTextureFromFileInMemoryEx(void* deviceRef, void* srcDataRef, int srcDataSize, int width, int height,
            int mipLevels, int usage, Format format, Pool pool, int filter, int mipFilter, 
            RawColorBGRA colorKey, void* srcInfoRef, PaletteEntry* paletteRef, void** textureOut);
    }
}
