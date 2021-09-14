using Riders.Tweakbox.Interfaces.Interfaces;
using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
public class RunningProperties
{
    /// <summary>
    /// Amount to add to the maximum speed until a switch to gear 2 is performed.
    /// </summary>
    public float? GearOneMaxSpeedOffset;

    /// <summary>
    /// Amount to add to the maximum speed of gear 2.
    /// </summary>
    public float? GearTwoMaxSpeedOffset;

    /// <summary>
    /// Amount to add to the maximum speed of gear 3.
    /// </summary>
    public float? GearThreeMaxSpeedOffset;

    /// <summary>
    /// Amount to add to the acceleration of gear 1.
    /// </summary>
    public float? GearOneAccelerationOffset;

    /// <summary>
    /// Amount to add to the acceleration of gear 2.
    /// </summary>
    public float? GearTwoAccelerationOffset;

    /// <summary>
    /// Amount to add to the acceleration of gear 3. (When exceeding GearTwoMaxSpeed)
    /// </summary>
    public float? GearThreeAccelerationOffset;

    /// <summary>
    /// Allows you to override running properties to be used for the gear.
    /// Note: The IntPtr is a pointer to running properties, cast it to `Sewer56.SonicRiders.Structures.Gameplay.RunningPhysics2*` and modify properties.
    /// DO NOT MODIFY THE POINTER
    /// </summary>
    public SetValueHandler<IntPtr> SetRunningProperties;
}