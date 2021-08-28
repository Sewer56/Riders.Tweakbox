using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Enums;
using Riders.Tweakbox.Services.Texture.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Riders.Tweakbox.Services.Texture.Interfaces
{
    public class TextureDictionaryBase : ITextureDictionary
    {
        // DO NOT CHANGE
        private const int HashLength = 16;

        /// <summary>
        /// Maps texture hashes to new file paths.
        /// </summary>
        internal Dictionary<string, TextureFile> Redirects { get; set; } = new Dictionary<string, TextureFile>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Maps texture hashes to animated textures.
        /// </summary>
        internal Dictionary<string, AnimatedTexture> AnimatedRedirects { get; set; } = new Dictionary<string, AnimatedTexture>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool TryGetTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            if (TryGetNormalTexture(xxHash, out data, out info))
                return true;

            if (TryGetAnimatedTexture(xxHash, out data, out info))
                return true;

            info = default;
            data = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetTextureInfo(string xxHash, out TextureInfo info)
        {
            if (TryGetNormalTextureInfo(xxHash, out info))
                return true;

            if (TryGetAnimatedTextureInfo(xxHash, out info))
                return true;

            info = default;
            return false;
        }

        protected bool TryGetAnimatedTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            info = default;
            data = default;
            if (!AnimatedRedirects.TryGetValue(xxHash, out var animTexture))
                return false;

            data = animTexture.GetFirstTexture(out var filePath);
            info = new TextureInfo(filePath, TextureType.Animated, animTexture);
            return true;
        }

        protected bool TryGetAnimatedTextureInfo(string xxHash, out TextureInfo info)
        {
            info = default;
            if (!AnimatedRedirects.TryGetValue(xxHash, out var animTexture))
                return false;

            animTexture.GetFirstTexturePath(out var filePath);
            info = new TextureInfo(filePath, TextureType.Animated, animTexture);
            return true;
        }

        protected bool TryGetNormalTexture(string xxHash, out TextureRef data, out TextureInfo info)
        {
            info = default;
            data = default;
            if (!Redirects.TryGetValue(xxHash, out var redirect))
                return false;

            data = TextureRef.FromFile(redirect.Path, redirect.Format);
            info = new TextureInfo(redirect.Path, TextureType.Normal);
            return true;
        }

        protected bool TryGetNormalTextureInfo(string xxHash, out TextureInfo info)
        {
            info = default;
            if (!Redirects.TryGetValue(xxHash, out var redirect))
                return false;

            info = new TextureInfo(redirect.Path, TextureType.Normal);
            return true;
        }

        protected bool TryAddTextureFromFilePath(string file, out TextureFile result, out string hash)
        {
            result = default;
            hash = default;

            var type = file.GetTextureFormatFromFileName();
            if (type == TextureFormat.Unknown)
                return false;

            // Extract hash from filename.
            var fileName = Path.GetFileName(file);
            var indexOfHash = fileName.IndexOf('_');
            if (indexOfHash == -1)
                return false;

            hash = fileName.Substring(indexOfHash + 1, HashLength);
            result = new TextureFile() { Path = file, Format = type };
            return true;
        }

        protected bool TryAddTextureFromFilePath(string file, string hash, out TextureFile result)
        {
            result = default;
            var type = file.GetTextureFormatFromFileName();
            if (type == TextureFormat.Unknown)
                return false;

            result = new TextureFile() { Path = file, Format = type };
            return true;
        }

        protected bool TryMakeAnimatedTextureFromFolder(string folder, out AnimatedTexture result)
        {
            result = default;

            try
            {
                return AnimatedTexture.TryCreate(folder, out result);
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool TryMakeAnimatedTextureFromFolder(string folder, out AnimatedTexture result, out string hash)
        {
            hash   = default;
            result = default;

            // Extract hash from filename.
            var folderName = Path.GetFileName(folder);
            var indexOfHash = folderName.IndexOf('_');
            if (indexOfHash == -1)
                return false;

            try
            {
                hash = folderName.Substring(indexOfHash + 1, HashLength);
                return AnimatedTexture.TryCreate(folder, out result);
            }
            catch (Exception) 
            {
                return false;
            }
        }
    }
}
