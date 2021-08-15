using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture.Structs;

/// <summary>
/// Represents an individual texture file inside an animated texture.
/// </summary>
public struct AnimatedTextureFile
{
    /// <summary>
    /// Path relative to the animated texture folder.
    /// </summary>
    public string RelativePath;

    /// <summary>
    /// The texture format.
    /// </summary>
    public TextureFormat Format;

    public AnimatedTextureFile(string relativePath, TextureFormat format)
    {
        RelativePath = relativePath;
        Format = format;
    }
}
