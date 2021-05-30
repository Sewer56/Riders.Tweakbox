using System;

namespace Riders.Tweakbox.Services.Texture.Enums
{
    public enum TextureFormat
    {
        None,
        Png,
        Dds,
        DdsLz4
    }

    public static class TextureFormatExtensions
    {
        /// <summary>
        /// Gets the redirect type given a file name or path.
        /// </summary>
        public static TextureFormat GetTextureFormatFromFileName(this string file)
        {
            if (file.EndsWith(TextureCommon.DdsExtension, StringComparison.OrdinalIgnoreCase))
                return TextureFormat.Dds;

            if (file.EndsWith(TextureCommon.PngExtension, StringComparison.OrdinalIgnoreCase))
                return TextureFormat.Png;

            if (file.EndsWith(TextureCommon.DdsLz4Extension, StringComparison.OrdinalIgnoreCase))
                return TextureFormat.DdsLz4;

            return TextureFormat.None;
        }
    }
}