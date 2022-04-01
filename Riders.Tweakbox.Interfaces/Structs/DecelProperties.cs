using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Riders.Tweakbox.Interfaces.Structs;

// TODO: Remove this when removing legacy physics config support
[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public struct DecelProperties
{
    // Reminder: Update DashPanelPropertiesSerializer in Riders.Tweakbox if updating.

    [FieldOffset(0)]
    public DecelMode Mode;

    [FieldOffset(4)]
    public float LinearSpeedCapOverride;

    [FieldOffset(8)]
    public float LinearMaxSpeedOverCap;

    public static DecelProperties GetDefault() => new DecelProperties()
    {
        Mode = DecelMode.Default,
        LinearSpeedCapOverride = 0.926000f, // ~200
        LinearMaxSpeedOverCap  = 0.87041667f
    };
}

public enum DecelMode : byte
{
    Default,
    Linear,
}
