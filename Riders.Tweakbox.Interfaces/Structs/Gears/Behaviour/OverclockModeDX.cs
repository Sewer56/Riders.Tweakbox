namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public struct OverclockModeDX
{
    /// <summary>
    /// True if enabled, else false.
    /// </summary>
    public bool IsEnabled;

    /// <summary>
    /// The mode under which overclock activates.
    /// </summary>
    public OverclockActivationMode ActivationMode;

    /// <summary>
    /// Amount of rings needed to activate this state if <see cref="ActivationMode"/> is Ring.
    /// </summary>
    public int RingActivation;

    /// <summary>
    /// The top speed of the gear. Using speedometer value.
    /// </summary>
    public float TopSpeed;

    /// <summary>
    /// The gear's boost speed. Using speedometer value.
    /// </summary>
    public float BoostSpeed;

    /// <summary>
    /// The boost cost of the gear; in air.
    /// </summary>
    public float BoostCost;

    /// <summary>
    /// Amount of frames needed to generate a drift dash.
    /// </summary>
    public int DriftDashFrames;

    /// <summary>
    /// Sets the drift cost for the gear.
    /// </summary>
    public float DriftCost;

    /// <summary>
    /// The amount it costs to perform a tornado.
    /// </summary>
    public float TornadoCost;

    /// <summary>
    /// The current acceleration of the gear.
    /// </summary>
    public float Acceleration;

    /// <summary>
    /// Air gain on attack.
    /// </summary>
    public float AttackAirGain;
}

public enum OverclockActivationMode
{
    Ring
}