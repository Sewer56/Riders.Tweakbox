namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Defines different properties which affect how cruising is performed on a gear.
/// </summary>
public struct CruisingProperties
{
    public bool Enabled;

    /// <summary>
    /// The amount of top speed gained per individual ring.
    /// </summary>
    public float? TopSpeedPerRing;
}
