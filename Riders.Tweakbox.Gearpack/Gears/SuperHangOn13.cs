using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class SuperHangOn13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "SuperHangOn 1.3";

    private AirProperties _airProperties = new AirProperties()
    {
        PitAirGainMultiplier = 3.00f
    };

    // IExtremeGear API Callbacks //
    public AirProperties GetAirProperties() => _airProperties;
}