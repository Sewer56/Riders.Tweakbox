using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class TheCrazyDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "TheCrazy DX";

    private AirProperties _airProperties = new AirProperties()
    {
        GainsRingsOnRingGear = true
    };

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };
    
    // IExtremeGear API Callbacks //
    public AirProperties GetAirProperties() => _airProperties;

    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;
}
