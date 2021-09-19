using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class PowerfulGear13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "PowerfulGear 1.3";

    private OffRoadProperties _offRoadProperties = new OffRoadProperties()
    {
        IgnoreSpeedLoss = true
    };

    // IExtremeGear API Callbacks //
    public OffRoadProperties GetOffroadProperties() => _offRoadProperties;
}
