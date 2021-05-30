using System.Buffers;
using System.IO;
using Riders.Tweakbox.Services.Texture.Enums;

namespace Riders.Tweakbox.Services.Texture
{
    /// <summary>
    /// References texture data.
    /// </summary>
    public ref struct TextureRef
    {
        public byte[] Data;
        public ArrayPool<byte> Owner;
        public bool NeedsDispose;

        /// <inheritdoc />
        public TextureRef(byte[] data, ArrayPool<byte> owner) : this()
        {
            Data = data;
            Owner = owner;
            NeedsDispose = true;
        }

        /// <inheritdoc />
        public TextureRef(byte[] data) : this() => Data = data;

        public void Dispose()
        {
            if (NeedsDispose)
                Owner.Return(Data);
        }

        /// <summary>
        /// Gets a texture reference from file.
        /// </summary>
        /// <param name="filePath">The file path to the texture.</param>
        /// <param name="type">The texture type.</param>
        public static TextureRef FromFile(string filePath, TextureFormat type)
        {
            return type == TextureFormat.DdsLz4 ? TextureCompression.PickleFromFile(filePath) : new TextureRef(File.ReadAllBytes(filePath));
        }
    }
}