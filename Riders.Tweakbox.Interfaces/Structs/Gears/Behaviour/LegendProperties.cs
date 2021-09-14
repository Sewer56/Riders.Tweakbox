using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Additional properties tied to the legend effect.
/// </summary>
public class LegendProperties
{
    /// <summary>
    /// Ignores legend effect specific states.
    /// </summary>
    public PlayerStateFlags IgnoreOnState;

    /// <summary>
    /// Allows you to override the drift dash cap for this gear.
    /// True means force legend, False means force not legend.
    /// Indeterminate means do nothing.
    /// </summary>
    public QueryValueHandler<QueryResult> OverrideLegendEffect;
}
