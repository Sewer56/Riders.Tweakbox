using Riders.Tweakbox.Services.Texture.Enums;
namespace Riders.Tweakbox.Services.Texture.Structs;

/// <summary>
/// Represents an individual texture file.
/// </summary>
public struct TextureFile
{
    /// <summary>
    /// Path to the texture.
    /// </summary>
    public string Path;

    /// <summary>
    /// The texture format.
    /// </summary>
    public TextureFormat Format;
}
