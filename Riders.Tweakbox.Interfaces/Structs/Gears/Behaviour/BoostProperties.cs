using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public class BoostProperties
{
    /// <summary>
    /// Disables boosting; such that boosting can only be triggered by user code.
    /// </summary>
    public bool? CannotBoost;

    /// <summary>
    /// Checks if the player is allowed to boost.
    /// </summary>
    public QueryValueHandler<QueryResult> CheckIfCanBoost;

    /// <summary>
    /// Gets extra boost chain multiplier.
    /// Measured in decimal. Default BCM is 1.193 so a value of 0.001 here would make the BCM 1.194.
    /// </summary>
    public float? AddedBoostChainMultiplier;

    /// <summary>
    /// Allows you to set the added boost chain multiplier.
    /// </summary>
    public QueryValueHandler<float> GetAddedBoostChainMultiplier;

    /// <summary>
    /// The amount of boost speed to add (speedometer) for every ring the gear has.
    /// DX 1.0 Cover P uses 0.5 as the value.
    /// </summary>
    public float? AddedBoostSpeedFromRingCount;

    /// <summary>
    /// The amount of acceleration applied on every frame of the boost.
    /// This makes the boost increase speed as it reaches the end of the boost duration.
    /// DX 1.0 Cover S uses a value of 0.0166666.
    /// </summary>
    public float? BoostAcceleration;

    /// <summary>
    /// Allows you to add to the boost speed.
    /// </summary>
    public GetAddedBoostSpeed GetAddedBoostSpeed;

    /// <summary>
    /// Sets the current air to a specific percentage of max air after a boost has been performed.
    /// </summary>
    public float? AirPercentageOnBoost;

    /// <summary>
    /// Increased boost duration for each level.
    /// </summary>
    public int[] AddedBoostDuration;

    /// <summary>
    /// Allows you to specify added boost duration.
    /// Can also be used for attaching extra code (e.g. set air) on boost.
    /// </summary>
    public QueryValueHandler<int> GetAddedBoostDuration;

    /// <summary>
    /// Allows you to attach an event to the boost action.
    /// </summary>
    public ApiEventHandler OnBoost;

    /// <summary>
    /// Tries to obtain extra boost duration for a given level.
    /// </summary>
    public int GetExtraBoostDurationForLevel(int level)
    {
        if (AddedBoostDuration == null || AddedBoostDuration.Length == 0)
            return 0;

        return level < AddedBoostDuration.Length 
            ? AddedBoostDuration[level] 
            : AddedBoostDuration[^1];
    }
}

/// <summary>
/// Allows you to query an external source about its opinion on something.
/// </summary>
/// <param name="playerPtr">Pointer to the player struct.</param>
/// <param name="playerIndex">Index of the player in the player struct.</param>
/// <param name="framesBoosting">The amount of frames the player has been boosting.</param>
/// <param name="level">The gear level, starting with 0 for level 1.</param>
public delegate float GetAddedBoostSpeed(IntPtr playerPtr, int playerIndex, int framesBoosting, int level);