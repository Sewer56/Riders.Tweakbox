using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class SuperHangOnDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "SuperHangOn DX";

    private AirProperties _airProperties = new AirProperties()
    {
        PitAirGainMultiplier = 2.75f
    };

    // IExtremeGear API Callbacks //
    public AirProperties GetAirProperties() => _airProperties;
}