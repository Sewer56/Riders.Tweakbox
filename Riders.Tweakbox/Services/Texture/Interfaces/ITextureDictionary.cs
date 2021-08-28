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
    /// <param name="info">The information about the texture data.</param>
    bool TryGetTexture(string xxHash, out TextureRef data, out TextureInfo info);

    /// <summary>
    /// Tries to pull a texture info from this dictionary.
    /// </summary>
    /// <param name="xxHash">The hash.</param>
    /// <param name="info">The information about the texture data.</param>
    bool TryGetTextureInfo(string xxHash, out TextureInfo info);
}