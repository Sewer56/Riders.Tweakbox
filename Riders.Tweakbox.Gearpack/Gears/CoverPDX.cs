using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverPDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverP DX";

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
    
    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public CruisingProperties GetCruisingProperties() => _cruisingProperties;

    public BoostProperties GetBoostProperties() => _boostProperties;
}