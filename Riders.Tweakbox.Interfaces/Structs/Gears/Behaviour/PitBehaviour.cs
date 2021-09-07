namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Influences how a gear behaves when it enters a pit.
/// </summary>
public struct PitBehaviour
{
    /// <summary>
    /// True if enabled.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Multiplies the rate at which the gear gains air inside the pit.
    /// i.e. 2.0 means 2x speed.
    /// </summary>
    public float AirGainMultiplier;
}
