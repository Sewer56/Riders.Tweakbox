using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture.Structs;

public struct TextureInfo
{
    /// <summary>
    /// Full path to the texture to be loaded.
    /// </summary>
    public string Path;

    /// <summary>
    /// The type of texture involved, e.g. Normal, Animated.
    /// </summary>
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
