using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Defines how shortcuts are modified when used with a gear granting the same shortcut type
/// or a new shortcut type.
/// </summary>
public class MonoTypeShortcutBehaviourDX
{
    /// <summary>
    /// The amount of percent (decimal) to slow the shortcut down if the type granted by
    /// this gear is a new type.
    /// </summary>
    public float? NewTypeSpeedModifierPercent = 0.95f;

    /// <summary>
    /// The amount of percent (decimal) to speed the shortcut up if the type granted by
    /// this gear is a type the character already has.
    /// </summary>
    public float? ExistingTypeSpeedModifierPercent = 1.15f;

    /// <summary>
    /// Sets the speed modifier when the gear is a mono type.
    /// </summary>
    public SetValueHandler<float> SetSpeedModifierForMonoType;

    /// <summary>
    /// Sets the speed modifier when the gear is an existing type.
    /// </summary>
    public SetValueHandler<float> SetSpeedModifierForExistingType;
}
