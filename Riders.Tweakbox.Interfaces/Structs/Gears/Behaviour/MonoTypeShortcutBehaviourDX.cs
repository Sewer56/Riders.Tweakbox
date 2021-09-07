namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Defines how shortcuts are modified when used with a gear granting the same shortcut type
/// or a new shortcut type.
/// </summary>
public struct MonoTypeShortcutBehaviourDX
{
    /// <summary>
    /// True if used.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// The amount of percent (decimal) to slow the shortcut down if the type granted by
    /// this gear is a new type.
    /// </summary>
    public float NewTypeSpeedModifierPercent = 0.95f;

    /// <summary>
    /// The amount of percent (decimal) to speed the shortcut up if the type granted by
    /// this gear is a type the character already has.
    /// </summary>
    public float ExistingTypeSpeedModifierPercent = 1.15f;
}
