using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class LegendDX : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Legend DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public LegendProperties GetLegendProperties() => new LegendProperties()
    {
        Enabled = true,
        IgnoreOnState = PlayerStateFlags.Attacking | PlayerStateFlags.GettingAttacked
    };
}
