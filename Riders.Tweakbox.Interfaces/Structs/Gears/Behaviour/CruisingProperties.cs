namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Defines different properties which affect how cruising is performed on a gear.
/// </summary>
public struct CruisingProperties
{
    public bool Enabled;

    /// <summary>
    /// Gains top speed from having rings.
    /// </summary>
    public bool GainsTopSpeedFromRings;

    /// <summary>
    /// The amount of top speed gained per individual ring.
    /// </summary>
    public float TopSpeedPerRing;
}
