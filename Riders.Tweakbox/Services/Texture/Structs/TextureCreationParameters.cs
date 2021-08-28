using System;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace Riders.Tweakbox.Services.Texture.Structs
{
    public unsafe class TextureCreationParameters
    {
        /// <summary>
        /// The hash associated with this texture.
        /// </summary>
        public string Hash;

        /// <summary>
        /// Pointer to the native instance of the IDirect3DTexture9 interface.
        /// Stored in case <see cref="TextureOut"/> used a location on the stack.
        /// </summary>
        public IntPtr NativePointer;

        /// <summary>
        /// True if this is a custom texture, else false.
        /// </summary>
        public bool IsCustomTexture;

        /* Below, D3DXCreateTextureFromFileInMemoryEx parameters */

        /// <summary>
        /// Device associated with the texture.
        /// </summary>
        public IntPtr Device;

        /// <summary>
        /// Where the raw data for the texture was sourced from.
        /// Note: May no longer be valid.
        /// </summary>
        public byte* SrcDataRef;

        /// <summary>
        /// The size of the raw data for the texture.
        /// Note: May no longer be valid.
        /// </summary>
        public int SrcDataSize;

        /// <summary>
        /// Width of the texture.
        /// If this value is zero or D3DX_DEFAULT, the dimensions are taken from the file.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the texture.
        /// If this value is zero or D3DX_DEFAULT, the dimensions are taken from the file.
        /// </summary>
        public int Height;

        /// <summary>
        /// Number of mip levels requested.
        /// If this value is zero or D3DX_DEFAULT, a complete mipmap chain is created.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// 0, D3DUSAGE_RENDERTARGET, or D3DUSAGE_DYNAMIC.
        /// Setting this flag to D3DUSAGE_RENDERTARGET indicates that the surface is to be used as a render target.
        /// The resource can then be passed to the pNewRenderTarget parameter of the SetRenderTarget method.
        /// If either D3DUSAGE_RENDERTARGET or D3DUSAGE_DYNAMIC is specified, Pool must be set to D3DPOOL_DEFAULT, and the application
        /// should check that the device supports this operation by calling CheckDeviceFormat. For more information about using dynamic textures, see Using Dynamic Textures.
        /// </summary>
        public Usage Usage;

        /// <summary>
        /// Member of the D3DFORMAT enumerated type, describing the requested pixel format for the texture.
        /// The returned texture might have a different format from that specified by Format.
        /// Applications should check the format of the returned texture.
        /// If D3DFMT_UNKNOWN, the format is taken from the file.
        /// If D3DFMT_FROM_FILE, the format is taken exactly as it is in the file, and the call will fail if this violates device capabilities.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Doesn't really matter since we're using D3D9Ex.
        /// </summary>
        public Pool Pool;

        /// <summary>
        /// Combination of one or more flags controlling how the image is filtered.
        /// Specifying D3DX_DEFAULT for this parameter is the equivalent of specifying D3DX_FILTER_TRIANGLE | D3DX_FILTER_DITHER.
        /// Each valid filter must contain one of the flags in D3DX_FILTER.
        /// </summary>
        public int Filter;

        /// <summary>
        /// Combination of one or more flags controlling how the image is filtered.
        /// Specifying D3DX_DEFAULT for this parameter is the equivalent of specifying D3DX_FILTER_BOX.
        /// Each valid filter must contain one of the flags in D3DX_FILTER.
        /// In addition, use bits 27-31 to specify the number of mip levels to be skipped (from the top of the mipmap chain) when a .dds texture is loaded into memory; this allows you to skip up to 32 levels.
        /// </summary>
        public int MipFilter;

        /// <summary>
        /// D3DCOLOR value to replace with transparent black, or 0 to disable the colorkey.
        /// This is always a 32-bit ARGB color, independent of the source image format.
        /// Alpha is significant and should usually be set to FF for opaque color keys.
        /// Thus, for opaque black, the value would be equal to 0xFF000000.
        /// </summary>
        public RawColorBGRA ColorKey;

        /// <summary>
        /// Pointer to a D3DXIMAGE_INFO structure to be filled with a description of the data in the source image file, or NULL.
        /// </summary>
        public byte* SrcInfoRef;

        /// <summary>
        /// Pointer to a PALETTEENTRY structure, representing a 256-color palette to fill in, or NULL. See Remarks.
        /// </summary>
        public PaletteEntry* PaletteRef;

        /// <summary>
        /// Address of a pointer to an IDirect3DTexture9 interface, representing the created texture object.
        /// </summary>
        public byte** TextureOut;

        public TextureCreationParameters(string hash, IntPtr device, byte* srcDataRef, int srcDataSize, int width, int height, int mipLevels, Usage usage, Format format, Pool pool, int filter, int mipFilter, RawColorBGRA colorKey, byte* srcInfoRef, PaletteEntry* paletteref, byte** textureOut, bool isCustomTexture)
        {
            Hash = hash;
            Device = device;
            SrcDataRef = srcDataRef;
            SrcDataSize = srcDataSize;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            Usage = usage;
            Format = format;
            Pool = pool;
            Filter = filter;
            MipFilter = mipFilter;
            ColorKey = colorKey;
            SrcInfoRef = srcInfoRef;
            PaletteRef = paletteref;
            TextureOut = textureOut;
            NativePointer = (IntPtr)(*textureOut);
            IsCustomTexture = isCustomTexture;
        }
    }
}
