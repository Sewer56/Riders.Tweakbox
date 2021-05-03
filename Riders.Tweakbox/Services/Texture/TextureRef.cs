using System;
using System.Buffers;

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
    }
}