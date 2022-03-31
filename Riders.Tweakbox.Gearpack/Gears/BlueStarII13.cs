using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BlueStarII13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "BlueStarII 1.3";

    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostChainMultiplier = 0.05f,
        AddedBoostDuration = new[] { 0, 0, 30 }
    };

    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}
