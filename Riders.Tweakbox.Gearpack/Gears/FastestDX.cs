using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class FastestDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Fastest DX";

    private TornadoProperties _tornadoProperties = new TornadoProperties()
    {
        SpeedMultiplier = 0.5f
    };
    
    // IExtremeGear API Callbacks //
    public TornadoProperties GetTornadoProperties() => _tornadoProperties;
}
