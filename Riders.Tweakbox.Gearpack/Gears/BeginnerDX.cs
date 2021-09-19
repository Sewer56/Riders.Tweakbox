using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BeginnerDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Beginner DX";

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(310.0f)
    };

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;
}
