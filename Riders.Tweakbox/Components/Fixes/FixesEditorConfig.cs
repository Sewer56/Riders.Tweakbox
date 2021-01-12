using System;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Fixes
{
    public class FixesEditorConfig : IConfiguration
    {
        public Internal Data = Internal.GetDefault();

        // Serialization
        public byte[] ToBytes() => Json.SerializeStruct(ref Data);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Json.DeserializeStruct<Internal>(bytes);
            return bytes.Slice(Struct.GetSize<Internal>());
        }

        // Apply
        public void Apply() { }
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new FixesEditorConfig();

        #region Internal
        public struct Internal
        {
            public bool BootToMenu;
            public bool FramePacing;
            public bool FramePacingSpeedup; // Speed up game to compensate for lag.
            public float DisableYieldThreshold;
            public bool D3DDeviceFlags;

            internal static Internal GetDefault() => new Internal
            {
                BootToMenu = true,
                FramePacingSpeedup = true,
                FramePacing = true,
                DisableYieldThreshold = 80,
                D3DDeviceFlags = true
            };
        }
        #endregion
    }
}
