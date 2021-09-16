using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverPDX : CustomGearBase, IExtremeGear
{
    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(280.0f)
    };

    private CruisingProperties _cruisingProperties = new CruisingProperties()
    {
        TopSpeedPerRing = Utility.SpeedometerToFloat(0.5f)
    };

    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostSpeedFromRingCount = Utility.SpeedometerToFloat(0.5f),
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "CoverP DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public CruisingProperties GetCruisingProperties() => _cruisingProperties;

    public BoostProperties GetBoostProperties() => _boostProperties;
}