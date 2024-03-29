﻿using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Defines different properties which affect how cruising is performed on a gear.
/// </summary>
public class CruisingProperties
{
    /// <summary>
    /// The amount of top speed gained per individual ring.
    /// </summary>
    public float? TopSpeedPerRing;

    /// <summary>
    /// Allows you to modify the deceleration speed applied to the player.
    /// </summary>
    public SetValueHandler<float> SetDecelerationSpeed;
}
