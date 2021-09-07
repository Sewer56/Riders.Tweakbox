using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class TheCrazy13 : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "TheCrazy 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public AirProperties GetAirProperties() => new AirProperties()
    {
        Enabled = true,
        GainsRingsOnRingGear = true
    };

    public DriftDashProperties GetDriftDashProperties() => new DriftDashProperties()
    {
        Enabled = true,
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };
}
