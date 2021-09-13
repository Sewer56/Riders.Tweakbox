using Riders.Tweakbox.Interfaces.Interfaces;
using System;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public struct ItemProperties
{
    /// <summary>
    /// True if this struct is enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Allows you to modify the character's amount of rings right after a ring pickup occurs.
    /// </summary>
    public SetValueHandler<int> SetRingCountOnPickup;

}