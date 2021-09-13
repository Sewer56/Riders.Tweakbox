using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Changes how game behaviour is modified when a gear places a tornado.
/// </summary>
public struct TornadoProperties
{
    /// <summary>
    /// True if this property set is used.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Multiplier for the speed set when placing down a tornado.
    /// </summary>
    public float? SpeedMultiplier;

    /// <summary>
    /// Allows you to modify the tornado speed set when performing a tornado.
    /// </summary>
    public SetValueHandler<float> SetTornadoSpeed;
}
