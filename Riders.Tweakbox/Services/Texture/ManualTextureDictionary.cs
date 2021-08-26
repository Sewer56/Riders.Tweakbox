using Riders.Tweakbox.Services.Texture.Interfaces;

namespace Riders.Tweakbox.Services.Texture
{
    /// <summary>
    /// A variant of a texture dictionary that allows for internal code/API access.
    /// </summary>
    public class ManualTextureDictionary : TextureDictionaryBase
    {
        /// <summary>
        /// Tries to remove a texture.
        /// </summary>
        /// <param name="hash">Hash of the texture to remove.</param>
        public bool TryRemoveTexture(string hash) => Redirects.Remove(hash);

        /// <summary>
        /// Tries to remove an animated texture.
        /// </summary>
        /// <param name="hash">Hash of the texture to remove.</param>
        public bool TryRemoveAnimatedTexture(string hash) => AnimatedRedirects.Remove(hash);

        /// <summary>
        /// Attempts to add a texture from a given file path.
        /// </summary>
        /// <param name="filePath">The file path to make the texture from.</param>
        /// <param name="hash">The hash of the texture. Use this hash to remove it in the future, if necessary.</param>
        /// <returns>True if the operation suceeded, else false.</returns>
        public bool TryAddTextureFromFilePath(string filePath, string hash)
        {
            if (!TryAddTextureFromFilePath(filePath, hash, out var result))
                return false;

            Redirects[hash] = result;
            return true;
        }

        /// <summary>
        /// Attempts to add an animated texture from a given folder.
        /// </summary>
        /// <param name="folder">The folder to add animated textures from.</param>#
        /// <param name="hash">The hash of the texture. Use this hash to remove it in the future, if necessary.</param>
        /// <returns>True if the operation suceeded, else false.</returns>
        public bool TryAddAnimatedTextureFromFolder(string folder, string hash)
        {
            if (!TryMakeAnimatedTextureFromFolder(folder, out var result))
                return false;
                
            AnimatedRedirects[hash] = result;
            return true;
        }

        /// <summary>
        /// Resets all redirections made by this dictionary.
        /// </summary>
        public void Clear()
        {
            Redirects.Clear();
            AnimatedRedirects.Clear();
        }
    }
}
