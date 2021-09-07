using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies shortcut behaviour for a specific gear.
/// </summary>
public struct ShortcutBehaviour
{
    /// <summary>
    /// True if enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Decimal by which to multiply shortcut speed for speed types.
    /// </summary>
    public float SpeedShortcutModifier;

    /// <summary>
    /// Decimal by which to multiply shortcut speed for fly types.
    /// </summary>
    public float FlyShortcutModifier;

    /// <summary>
    /// [UNIMPLEMENTED]
    /// Decimal by which to multiply shortcut speed for power types.
    /// </summary>
    public float PowerShortcutModifier;

    /// <summary>
    /// Sets the shortcut speed for speed type.
    /// </summary>
    public SetValueHandler<float> SetSpeedShortcutSpeed;

    /// <summary>
    /// Sets the shortcut speed for fly type.
    /// </summary>
    public SetValueHandler<float> SetFlyShortcutSpeed;

    /// <summary>
    /// [UNIMPLEMENTED]
    /// Sets the shortcut speed for power type.
    /// </summary>
    public SetValueHandler<float> SetPowerShortcutSpeed;
}
