using Riders.Tweakbox.Services.Texture.Structs;
namespace Riders.Tweakbox.Services.Texture.Interfaces;

public interface ITextureDictionary
{
    /// <summary>
    /// Tries to pull a texture from this dictionary.
    /// Texture data is placed in the resulting output span.
    /// </summary>
    /// <param name="xxHash">The hash.</param>
    /// <param name="data">The texture data.</param>
    /// <param name="info">The file path from which the data was loaded.</param>
    bool TryGetTexture(string xxHash, out TextureRef data, out TextureInfo info);
}