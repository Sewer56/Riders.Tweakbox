using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class BlueStarIIDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "BlueStarII DX";

    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostChainMultiplier = 0.15f,
        AddedBoostDurationLv3 = 30
    };
    
    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}