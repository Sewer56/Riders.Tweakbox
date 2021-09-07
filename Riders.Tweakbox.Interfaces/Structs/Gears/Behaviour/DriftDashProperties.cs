namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;


public struct DriftDashProperties
{
    // TODO: ACCOUNT FOR OVERCLOCK MODE

    /// <summary>
    /// Enables this set of properties.
    /// </summary>
    public bool Enabled = false;

    /// <summary>
    /// Performs a boost when a drift dash is performed.
    /// </summary>
    public bool BoostOnDriftDash = false;

    /// <summary>
    /// Sets the maximum drift dash speed.
    /// </summary>
    public float? DriftDashCap;
}
