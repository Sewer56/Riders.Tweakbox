using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Nimbus13 : CustomGearBase, IExtremeGear
{
    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    private WallHitBehaviour _wallHitBehaviour = new WallHitBehaviour()
    {
        SpeedLossMultiplier = 0
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Nimbus 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public WallHitBehaviour GetWallHitBehaviour() => _wallHitBehaviour;
}