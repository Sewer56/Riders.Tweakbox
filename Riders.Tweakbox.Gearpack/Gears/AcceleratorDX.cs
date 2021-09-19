using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AcceleratorDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Accelerator DX";

    private AirProperties _airProperties = new AirProperties()
    {
        GainsRingsOnRingGear = true,
        PowerAirGainMultiplier = 0.5f,
    };

    private DriftDashProperties _dashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    // IExtremeGear API Callbacks //
    public AirProperties GetAirProperties() => _airProperties;

    public DriftDashProperties GetDriftDashProperties() => _dashProperties;
}