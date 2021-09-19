using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class SpeeedBalancerDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "SpeedBalancer DX";

    private WallHitBehaviour _wallHitBehaviour = new WallHitBehaviour()
    {
        SpeedLossMultiplier = 0
    };
    
    // IExtremeGear API Callbacks //
    public WallHitBehaviour GetWallHitBehaviour() => _wallHitBehaviour;
}