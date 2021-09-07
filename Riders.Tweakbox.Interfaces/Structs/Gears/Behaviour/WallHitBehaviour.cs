namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies behaviour of the gear when the gear encounters a wall.
/// </summary>
public struct WallHitBehaviour
{
    /// <summary>
    /// True if enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Flat amount of speed (percentage) to grant to the player on a hit.
    /// Applied after <see cref="SpeedLossMultiplier"/>.
    /// </summary>
    public float? SpeedGainFlat;

    /// <summary>
    /// The multiplier for the speed loss incurred after a wall hit.
    /// </summary>
    public float? SpeedLossMultiplier;
}
