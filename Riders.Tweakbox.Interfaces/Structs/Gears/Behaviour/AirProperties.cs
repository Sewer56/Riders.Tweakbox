using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies how ring gears behave.
/// </summary>
public struct AirProperties
{
    public bool Enabled;
    
    /// <summary>
    /// True if the gear should gain rings.
    /// </summary>
    public bool GainsRingsOnRingGear;

    /// <summary>
    /// Allows to override whether this gear should gain air for this frame.
    /// </summary>
    public QueryValueHandler<QueryResult> ShouldGainAir;

    /// <summary>
    /// Amount of air gained from speed type shortcuts.
    /// </summary>
    public float SpeedAirGain = 1;

    /// <summary>
    /// Amount of air gained from fly type shortcuts.
    /// </summary>
    public float FlyAirGain = 1;

    /// <summary>
    /// Amount of air gained from power type shortcuts.
    /// </summary>
    public float PowerAirGain = 1;
}
