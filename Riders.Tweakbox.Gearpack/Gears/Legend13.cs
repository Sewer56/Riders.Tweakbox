using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Legend13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Legend 1.3";

    private LegendProperties _legendProperties = new LegendProperties()
    {
        IgnoreOnState = PlayerStateFlags.Attacking | PlayerStateFlags.GettingAttacked
    };

    // IExtremeGear API Callbacks //
    public LegendProperties GetLegendProperties() => _legendProperties;
}