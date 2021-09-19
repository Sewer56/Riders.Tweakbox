﻿using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Fastest13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Fastest 1.3";

    private TornadoProperties _tornadoProperties = new TornadoProperties()
    {
        SpeedMultiplier = 0.5f
    };
    
    // IExtremeGear API Callbacks //
    public TornadoProperties GetTornadoProperties() => _tornadoProperties;
}
