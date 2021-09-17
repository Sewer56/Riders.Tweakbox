using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public class ExtendedLevelStats
{
    /// <summary>
    /// [CUSTOM GEARS ONLY. DO NOT USE ON CHARACTERS.]
    /// Overrides individual level information for each this gear's levels.
    /// Levels 3 and above will use Character Level 3's stats when calculating final stats.
    /// </summary>
    public ExtendedExtremeGearLevelStats[] ExtendedStats;

    /// <summary>
    /// Allows you to modify player stats for a given level.
    /// Executed every frame.
    /// </summary>
    public EditGearStatsHandler SetPlayerStats;

    /// <summary>
    /// Returns the player's current level. Including extended levels.
    /// </summary>
    /// <param name="rings">The amount of rings in the player's posession.</param>
    public byte? TryGetPlayerLevel(int rings)
    {
        if (ExtendedStats == null)
            return null;

        for (int x = 0; x < ExtendedStats.Length - 1; x++)
        {
            var currentRings = ExtendedStats[x].RingCount;
            var nextRings = ExtendedStats[x + 1].RingCount;

            if (rings >= currentRings && rings < nextRings)
                return (byte)x;
        }

        return (byte)(ExtendedStats.Length - 1);
    }
}

/// <summary>
/// Allows you the gear stats for a given level.
/// </summary>
/// <param name="levelStatsPtr">Pointer to the level data for the character. Cast this to `Sewer56.SonicRiders.Structures.Gameplay.PlayerLevelStats*`</param>
/// <param name="playerPtr">Pointer to the player struct.</param>
/// <param name="playerIndex">Index of the player in the player struct.</param>
/// <param name="playerLevel">The level of the player.</param>
public delegate void EditGearStatsHandler(IntPtr levelStatsPtr, IntPtr playerPtr, int playerIndex, int playerLevel);

public struct ExtendedExtremeGearLevelStats
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