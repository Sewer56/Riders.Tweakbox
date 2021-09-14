using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;


public class DriftDashProperties
{
    /// <summary>
    /// Performs a boost when a drift dash is performed.
    /// </summary>
    public bool BoostOnDriftDash = false;

    /// <summary>
    /// Sets the maximum drift dash speed.
    /// </summary>
    public float? DriftDashCap;

    /// <summary>
    /// Allows you to override the drift dash cap for this gear.
    /// </summary>
    public SetValueHandler<float> SetDriftDashCap;
}
