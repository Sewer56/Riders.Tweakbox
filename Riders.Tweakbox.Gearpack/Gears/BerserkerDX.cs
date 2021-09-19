using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BerserkerDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Berserker DX";

    private BerserkerPropertiesDX _berserkerProperties = new BerserkerPropertiesDX()
    {
        PassiveDrainIncreaseFlat = 224,
        TriggerPercentage = 0.75f,
        SpeedMultiplier = 1.0025f
    };

    // IExtremeGear API Callbacks //
    public BerserkerPropertiesDX GetBerserkerProperties() => _berserkerProperties;
}
