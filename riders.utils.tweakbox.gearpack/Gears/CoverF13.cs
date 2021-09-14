using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverF13 : CustomGearBase, IExtremeGear
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostChainMultiplier = -0.193f, // Disable BoostChain
        AirPercentageOnBoost = 0.7f
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "CoverF 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}