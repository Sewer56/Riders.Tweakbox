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
    /// Amount of speed (speedometer) to offset from the slowdown caused by placing a tornado.
    /// </summary>
    public float SpeedOffset;
}
