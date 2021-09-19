using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AccessDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Access DX";

    private DashPanelGearProperties _dashPanelGearProperties = new DashPanelGearProperties()
    {
        AdditionalSpeed = Utility.SpeedometerToFloat(10)
    };

    // IExtremeGear API Callbacks //
    public DashPanelGearProperties GetDashPanelProperties() => _dashPanelGearProperties;
}
