using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class MagicCarpet13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "MagicCarpet 1.3";

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    private OffRoadProperties _offRoadProperties = new OffRoadProperties()
    {
        IgnoreSpeedLoss = true
    };

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public OffRoadProperties GetOffroadProperties() => _offRoadProperties;
}