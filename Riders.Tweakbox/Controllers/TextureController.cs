using System;
using System.IO;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Services.Texture;
using Sewer56.SonicRiders;
using SharpDX;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class TextureController : IController
    {
        private TextureService _textureService = IoC.GetConstant<TextureService>();
        private TextureInjectionConfig _config = IoC.GetConstant<TextureInjectionConfig>();

        private IO _io;
        private IHook<D3DXCreateTextureFromFileInMemoryEx> _createTextureHook;
        
        public TextureController(IO io)
        {
            _io = io;
            var d3dx9Handle  = PInvoke.LoadLibrary("d3dx9_25.dll");
            var createTextureFromFileInMemoryEx = Native.GetProcAddress(d3dx9Handle, "D3DXCreateTextureFromFileInMemoryEx");
            _createTextureHook = SDK.ReloadedHooks.CreateHook<D3DXCreateTextureFromFileInMemoryEx>(CreateTextureFromFileInMemoryHook, (long) createTextureFromFileInMemoryEx).Activate();
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
            {
                // Texture Load
                var texture     = new Texture((IntPtr) (*textureout));

                // Hash The Texture
                var description = texture.GetLevelDescription(0);
                var fileName    = $"{description.Width}x{description.Height}_{xxHash}.png"; // DO NOT PREPEND, ONLY APPEND

                BaseTexture.ToFile(texture, Path.Combine(_io.TextureDumpFolder, fileName), ImageFileFormat.Png);
                Log.WriteLine($"Dumped Texture: {fileName}", LogCategory.TextureDump);
            }

            return result;
        }

        /// <inheritdoc />
        public void Disable() => _createTextureHook.Disable();

        /// <inheritdoc />
        public void Enable() => _createTextureHook.Enable();

        [Function(CallingConventions.Stdcall)]
        public unsafe delegate int D3DXCreateTextureFromFileInMemoryEx(void* deviceRef, void* srcDataRef, int srcDataSize, int width, int height,
                                                                        int mipLevels, int usage, Format format, Pool pool, int filter, int mipFilter, 
                                                                        RawColorBGRA colorKey, void* srcInfoRef, PaletteEntry* paletteRef, void** textureOut);
    }
}
