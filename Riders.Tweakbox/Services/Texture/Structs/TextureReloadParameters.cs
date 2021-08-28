namespace Riders.Tweakbox.Services.Texture.Structs
{
    /// <summary>
    /// Represents an individual request to reload a texture.
    /// </summary>
    internal unsafe class TextureReloadParameters
    {
        internal TextureRef Ref;
        internal TextureInfo Info;
        internal TextureCreationParameters Parameters;

        public TextureReloadParameters(TextureRef @ref, TextureInfo info, TextureCreationParameters parameters)
        {
            Ref = @ref;
            Info = info;
            Parameters = parameters;
        }
    }
}