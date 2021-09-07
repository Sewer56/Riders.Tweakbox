using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BlueStarII12 : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "BlueStarII 1.2"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public BoostProperties GetBoostProperties() => new BoostProperties()
    {
        Enabled = true,
        AddedBoostDurationLv3 = 30
    };
}
