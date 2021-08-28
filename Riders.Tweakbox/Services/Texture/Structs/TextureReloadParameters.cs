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
        public bool CustomTextureDataInitialized { get; private set; }

        public TextureReloadParameters(TextureRef @ref, TextureInfo info, TextureCreationParameters parameters)
        {
            Ref = @ref;
            Info = info;
            Parameters = parameters;
            CustomTextureDataInitialized = true;
        }

        public TextureReloadParameters(TextureCreationParameters parameters)
        {
            Parameters = parameters;
            CustomTextureDataInitialized = false;
        }
    }
}