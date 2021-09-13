using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AdvantageS13 : CustomGearBase, IExtremeGear
{
    private RunningProperties _runningProperties;

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        _runningProperties = new RunningProperties()
        {
            Enabled = true,
            SetRunningProperties = SetRunningProperties
        };

        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "AdvantageS 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    private unsafe IntPtr SetRunningProperties(IntPtr playerPtr, int playerIndex, int playerLevel, IntPtr runningPhysics)
    {
        var runPhysics = (RunningPhysics2*)runningPhysics;
        runPhysics->GearOneAcceleration = (float)(0.006172839087 * 3f);
        switch (playerLevel)
        {
            case 0:
                runPhysics->GearThreeMaxSpeed = Utility.SpeedometerToFloat(190.0f);
                break;

            case 1:
                runPhysics->GearThreeMaxSpeed = Utility.SpeedometerToFloat(220.0f);
                break;

            case >= 2:
                runPhysics->GearThreeMaxSpeed = Utility.SpeedometerToFloat(240.0f);
                break;
        }

        return runningPhysics;
    }

    // API Implementation
    public RunningProperties GetRunningProperties() => _runningProperties;
}