namespace Riders.Tweakbox.Interfaces;

public static class Utility
{
    private const float SpeedToSpeedometerRatio = 216.0f;

    /// <summary>
    /// Converts a speed in float to its value in-game on the speedometer.
    /// </summary>
    /// <param name="speed">The speed to convert to speedometer.</param>
    public static float SpeedToSpeedometer(float speed) => speed * SpeedToSpeedometerRatio;

    /// <summary>
    /// Converts a speed in speedometer to its true float value.
    /// </summary>
    /// <param name="speed">The speed to convert to float.</param>
    public static float SpeedometerToFloat(float speed) => speed / SpeedToSpeedometerRatio;
}
