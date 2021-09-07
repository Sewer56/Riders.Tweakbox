namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Changes how collecting rings affects the current state of the game.
/// </summary>
public struct RingCollectionProperties
{
    /// <summary>
    /// True if enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Multiplies the amount of rings gained from picking up a single ring.
    /// </summary>
    public float RingGainMultiplier;
}
