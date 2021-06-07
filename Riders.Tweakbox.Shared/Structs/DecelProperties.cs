using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Riders.Tweakbox.Shared.Structs
{
    // Size contains extra space for future additions.
    [StructLayout(LayoutKind.Explicit, Size = 0x20)] 
    public struct DecelProperties
    {
        [FieldOffset(0)]
        public DecelMode Mode;

        [FieldOffset(1)] 
        public float LinearSpeedCapOverride;

        public static DecelProperties GetDefault() => new DecelProperties()
        {
            Mode = DecelMode.Default,
            LinearSpeedCapOverride = 0.926000f // ~200
        };
    }

    public enum DecelMode : byte
    {
        Default,
        Linear,

    }
}