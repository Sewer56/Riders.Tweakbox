using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverP13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverP 1.3";

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(310.0f)
    };

    private CruisingProperties _cruisingProperties = new CruisingProperties()
    {
        TopSpeedPerRing = Utility.SpeedometerToFloat(0.5f)
    };
    
    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public CruisingProperties GetCruisingProperties() => _cruisingProperties;
}