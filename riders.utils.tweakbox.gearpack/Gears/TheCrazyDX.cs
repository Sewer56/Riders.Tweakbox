using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class TheCrazyDX : CustomGearBase, IExtremeGear
{
    private AirProperties _airProperties = new AirProperties()
    {
        GainsRingsOnRingGear = true
    };

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "TheCrazy DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public AirProperties GetAirProperties() => _airProperties;

    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;
}
