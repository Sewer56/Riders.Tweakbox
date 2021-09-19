using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BlueStarII12 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "BlueStarII 1.2";

    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDurationLv3 = 30
    };

    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}
