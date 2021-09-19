using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Accelerator13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Accelerator 1.3";

    private DriftDashProperties _properties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f)
    };

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _properties;
}