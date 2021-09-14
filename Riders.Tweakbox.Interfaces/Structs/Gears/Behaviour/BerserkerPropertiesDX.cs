namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Properties for DX Berserker Mode.
/// </summary>
public class BerserkerPropertiesDX
{
    /// <summary>
    /// Percentage of air above which berserk mode is triggered.
    /// </summary>
    public float TriggerPercentage = 75.0f;

    /// <summary>
    /// The amount by which to multiply current speed when in berserk mode.
    /// </summary>
    public float SpeedMultiplier = 1.02f;

    /// <summary>
    /// The amount by which passive drain is increased.
    /// </summary>
    public int PassiveDrainIncreaseFlat = 224;
}
