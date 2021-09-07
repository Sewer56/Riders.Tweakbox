using Riders.Tweakbox.Interfaces.Structs.Enums;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Additional properties tied to the legend effect.
/// </summary>
public struct LegendProperties
{
    /// <summary>
    /// True if enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Ignores legend effect specific states.
    /// </summary>
    public PlayerStateFlags IgnoreOnState;
}
