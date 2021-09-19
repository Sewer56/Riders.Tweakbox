using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Cannonball13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Cannonball 1.3";

    private HandlingProperties _handlingProperties = new HandlingProperties()
    {
        SpeedLossMultiplier = 0
    };

    // IExtremeGear API Callbacks //
    public HandlingProperties GetHandlingProperties() => _handlingProperties;
}
