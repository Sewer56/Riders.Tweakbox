using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class PowerGearDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "PowerGear DX";

    private CruisingProperties _cruisingProperties = new() { SetDecelerationSpeed = SetDecelerationSpeed };

    private static float SetDecelerationSpeed(IntPtr playerptr, int playerindex, int playerlevel, float value)
    {
        if (value > 0)
            return value * 0.75f;

        return value;
    }

    // IExtremeGear API Callbacks //
    public CruisingProperties GetCruisingProperties() => new () { SetDecelerationSpeed = SetDecelerationSpeed };
}