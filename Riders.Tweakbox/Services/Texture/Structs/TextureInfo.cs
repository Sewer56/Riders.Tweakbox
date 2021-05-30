using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Enums;

namespace Riders.Tweakbox.Services.Texture.Structs
{
    /// <summary>
    /// Contains information about a loaded texture.
    /// </summary>
    public struct TextureInfo
    {
        public string Path;
        public TextureType Type;

        /// <summary>
        /// The animated texture instance.
        /// This field is only valid if the value of <see cref="Type"/> is <see cref="TextureType.Animated"/>.
        /// </summary>
        public AnimatedTexture Animated;

        public TextureInfo(string path, TextureType type)
        {
            Path = path;
            Type = type;
            Animated = null;
        }

        public TextureInfo(string path, TextureType type, AnimatedTexture animated)
        {
            Path = path;
            Type = type;
            Animated = animated;
        }
    }
}