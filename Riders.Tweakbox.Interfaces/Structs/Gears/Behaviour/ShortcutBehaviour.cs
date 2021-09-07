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
    /// Percentage by which to increase or decrease shortcut speed for speed types.
    /// </summary>
    public float SpeedShortcutModifier;

    /// <summary>
    /// Percentage by which to increase or decrease shortcut speed for fly types.
    /// </summary>
    public float FlyShortcutModifier;

    /// <summary>
    /// Percentage by which to increase or decrease shortcut speed for power types.
    /// </summary>
    public float PowerShortcutModifier;
}
