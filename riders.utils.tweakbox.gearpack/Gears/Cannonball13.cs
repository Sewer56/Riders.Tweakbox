using System.IO;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Cannonball13 : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Cannonball 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public HandlingProperties GetHandlingProperties() => new HandlingProperties()
    {
        Enabled = true,
        SpeedLossMultiplier = 0
    };
}
