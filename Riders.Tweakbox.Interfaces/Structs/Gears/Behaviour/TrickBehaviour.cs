using Riders.Tweakbox.Interfaces.Interfaces;
using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public class TrickBehaviour
{
    /// <summary>
    /// Flat amount of speed (percentage) to grant to the player on a trick land.
    /// </summary>
    public float? SpeedGainFlat;

    /// <summary>
    /// The multiplier of speed (percentage) to multiply the trick land speed by.
    /// </summary>
    public float? SpeedGainPercentage;

    /// <summary>
    /// Sets the speed gain for landing a trick.
    /// </summary>
    public SetValueHandler<float> SetSpeedGain;
}
