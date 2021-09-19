using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AdvantageF13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "AdvantageF 1.3";

    private WallHitBehaviour _wallHitBehaviour = new WallHitBehaviour()
    {
        SpeedLossMultiplier = -8.5f
    };
    
    // IExtremeGear API Callbacks //
    public WallHitBehaviour GetWallHitBehaviour() => _wallHitBehaviour;
}