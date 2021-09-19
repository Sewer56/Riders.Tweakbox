using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverF13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverF 1.3";

    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostChainMultiplier = -0.193f, // Disable BoostChain
        AirPercentageOnBoost = 0.7f
    };

    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}