namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public struct BoostProperties
{
    /// <summary>
    /// True if enabled.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Disables boosting; such that boosting can only be triggered by user code.
    /// </summary>
    public bool CannotBoost;

    /// <summary>
    /// Gets extra boost chain multiplier.
    /// Measured in decimal. Default BCM is 1.193 so a value of 0.001 here would make the BCM 1.194.
    /// </summary>
    public float? AddedBoostChainMultiplier;

    /// <summary>
    /// The amount of boost speed to add (speedometer) for every ring the gear has.
    /// DX 1.0 Cover P uses 0.5 as the value.
    /// </summary>
    public float? AddedBoostSpeedFromRingCount;

    /// <summary>
    /// The amount of acceleration (speedometer units) applied on every frame of the boost.
    /// This makes the boost increase speed as it reaches the end of the boost duration.
    /// DX 1.0 Cover S uses a value of 0.0166666.
    /// </summary>
    public float? BoostAcceleration;
    
    /// <summary>
    /// Sets the current air to a specific percentage of max air after a boost has been performed.
    /// </summary>
    public float? AirPercentageOnBoost;
}
