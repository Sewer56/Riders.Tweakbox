using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverFDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverF DX";

    private DriftDashProperties _driftDashProperties = new DriftDashProperties()
    {
        DriftDashCap = Utility.SpeedometerToFloat(260.0f),
        BoostOnDriftDash = true
    };

    private BoostProperties _boostProperties = new BoostProperties()
    {
        CannotBoost = true,
    };

    // IExtremeGear API Callbacks //
    public DriftDashProperties GetDriftDashProperties() => _driftDashProperties;

    public BoostProperties GetBoostProperties() => _boostProperties;
}