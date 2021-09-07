using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverS13 : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "CoverS 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public ShortcutBehaviour GetShortcutBehaviour() => new ShortcutBehaviour()
    {
        Enabled = true,
        FlyShortcutModifier = 1.075f,
        SpeedShortcutModifier = 1.075f,
        PowerShortcutModifier = 1.075f,
    };
}