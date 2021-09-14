using Riders.Tweakbox.Interfaces.Interfaces;
using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies behaviour of the gear when the gear encounters a wall.
/// </summary>
public class WallHitBehaviour
{
    /// <summary>
    /// Flat amount of speed (percentage) to grant to the player on a hit.
    /// Applied after <see cref="SpeedLossMultiplier"/>.
    /// </summary>
    public float? SpeedGainFlat;

    /// <summary>
    /// The multiplier for the speed loss incurred after a wall hit.
    /// </summary>
    public float? SpeedLossMultiplier;

    /// <summary>
    /// Allows you to override the amount of speed lost upon hitting a wall.
    /// Passed in parameter is the calculated speed loss.
    /// Hint: `player->WallBounceAngle`
    /// </summary>
    public SetValueHandler<float> SetSpeedLossOnWallHit;
}
