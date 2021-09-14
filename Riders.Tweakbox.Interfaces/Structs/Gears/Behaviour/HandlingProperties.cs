using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Changes how handling affects the gear.
/// </summary>
public class HandlingProperties
{
    /// <summary>
    /// Multiplies the amount of speed lost when the gear is turning.
    /// </summary>
    public float SpeedLossMultiplier;

    /// <summary>
    /// Allows you to override the speed loss value on turning.
    /// </summary>
    public SetValueHandler<float> SetSpeedLoss;
}
