using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Nimbus13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Nimbus 1.3";

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    private WallHitBehaviour _wallHitBehaviour = new WallHitBehaviour()
    {
        SpeedLossMultiplier = 0
    };

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public WallHitBehaviour GetWallHitBehaviour() => _wallHitBehaviour;
}