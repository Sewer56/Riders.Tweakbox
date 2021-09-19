using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverSDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverS DX";

    private BoostProperties _boostProperties = new BoostProperties()
    {
        BoostAcceleration = Utility.SpeedometerToFloat(10f / 60f)
    };
    
    // IExtremeGear API Callbacks //
    public BoostProperties GetBoostProperties() => _boostProperties;
}