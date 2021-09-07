using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BlueStarIIDX : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "BlueStarII DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public BoostProperties GetBoostProperties() => new BoostProperties()
    {
        Enabled = true,
        AddedBoostChainMultiplier = 0.15f,
        AddedBoostDurationLv3 = 30
    };
}