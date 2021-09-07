namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public struct ExtremeGearLevelStats
{
    /// <summary>
    /// Amount of rings necessary to enter this level state.
    /// </summary>
    public int RingCount;

    /// <summary>
    /// Maximum air at this level.
    /// </summary>
    public int MaxAir;

    /// <summary>
    /// Counted per frame.
    /// </summary>
    public int PassiveAirDrain;  

    /// <summary>
    /// Counted per frame.
    /// </summary>
    public int DriftAirCost;

    public int BoostCost;
    public int TornadoCost;
    public float SpeedGainedFromDriftDash;
    public float BoostSpeed;

    /// <summary>
    /// Gets extra boost chain multiplier.
    /// Measured in decimal. Default BCM is 1.193 so a value of 0.001 here would make the BCM 1.194.
    /// </summary>
    public float BoostChainOffset;
}
