using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BerserkerDX : CustomGearBase, IExtremeGear
{
    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Berserker DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }
    
    public BerserkerPropertiesDX GetBerserkerProperties() => new BerserkerPropertiesDX()
    {
        Enabled = true,
        PassiveDrainIncreaseFlat = 224,
        TriggerPercentage = 0.75f,
        SpeedMultiplier = 1.0025f
    };
}
