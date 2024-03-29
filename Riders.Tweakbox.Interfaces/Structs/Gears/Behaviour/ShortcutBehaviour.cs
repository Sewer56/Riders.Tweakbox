﻿using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies shortcut behaviour for a specific gear.
/// </summary>
public class ShortcutBehaviour
{
    /// <summary>
    /// Decimal by which to multiply shortcut speed for speed types.
    /// </summary>
    public float SpeedShortcutModifier;

    /// <summary>
    /// Decimal by which to multiply shortcut speed for fly types.
    /// </summary>
    public float FlyShortcutModifier;

    /// <summary>
    /// Speed to add to character when punching object as power type.
    /// </summary>
    public float PowerShortcutAddedSpeed;

    /// <summary>
    /// Sets the shortcut speed for speed type.
    /// </summary>
    public SetValueHandler<float> SetSpeedShortcutSpeed;

    /// <summary>
    /// Sets the shortcut speed for fly type.
    /// </summary>
    public SetValueHandler<float> SetFlyShortcutSpeed;

    /// <summary>
    /// Adds speed to character when punching object as power type.
    /// </summary>
    public SetValueHandler<float> AddPowerShortcutSpeed;
}
