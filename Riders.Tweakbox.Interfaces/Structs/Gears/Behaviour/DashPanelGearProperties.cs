using Riders.Tweakbox.Interfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

/// <summary>
/// Modifies how dash panels are affected by this gear.
/// </summary>
public struct DashPanelGearProperties
{
    /// <summary>
    /// True if dash panel edits are enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Additional speed gained from hitting a dash panel.
    /// </summary>
    public float? AdditionalSpeed;

    /// <summary>
    /// Sets the speed gain for hitting a dash panel.
    /// </summary>
    public SetValueHandler<float> SetSpeedGain;
}
