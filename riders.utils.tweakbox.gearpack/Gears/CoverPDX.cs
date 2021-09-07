using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverPDX : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "CoverP DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public DriftDashProperties GetDriftDashProperties() => new DriftDashProperties()
    {
        Enabled = true,
        DriftDashCap = Utility.SpeedometerToFloat(280.0f)
    };

    public CruisingProperties GetCruisingProperties() => new CruisingProperties()
    {
        Enabled = true,
        TopSpeedPerRing = Utility.SpeedometerToFloat(0.5f)
    };

    public BoostProperties GetBoostProperties() => new BoostProperties()
    {
        Enabled = true,
        AddedBoostSpeedFromRingCount = Utility.SpeedometerToFloat(0.5f),
    };
}