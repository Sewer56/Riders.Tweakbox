namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Influences how a gear behaves when it enters a pit.
/// </summary>
public class PitBehaviour
{
    /// <summary>
    /// Multiplies the rate at which the gear gains air inside the pit.
    /// i.e. 2.0 means 2x speed.
    /// </summary>
    public float AirGainMultiplier;
}
