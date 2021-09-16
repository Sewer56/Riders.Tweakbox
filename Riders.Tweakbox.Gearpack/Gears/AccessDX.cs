using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AccessDX : CustomGearBase, IExtremeGear
{
    private DashPanelGearProperties _dashPanelGearProperties = new DashPanelGearProperties()
    {
        AdditionalSpeed = Utility.SpeedometerToFloat(10)
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Access DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public DashPanelGearProperties GetDashPanelProperties() => _dashPanelGearProperties;
}
