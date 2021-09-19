using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AdvantageS13 : CustomGearBase, IExtremeGear
{
    private RunningProperties _runningProperties;

    public override string FolderName { get; set; } = "AdvantageS 1.3";

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void InitializeGear(string gearsFolder, Interfaces.ICustomGearApi gearApi)
    {
        _runningProperties = new RunningProperties()
        {
            SetRunningProperties = SetRunningProperties
        };
    }

    private unsafe IntPtr SetRunningProperties(IntPtr playerPtr, int playerIndex, int playerLevel, IntPtr runningPhysics)
    {
        var runPhysics = (RunningPhysics2*)runningPhysics;
        runPhysics->GearOneAcceleration   = (float)(0.006172839087 * 3f);
        runPhysics->GearTwoAcceleration   = (float)(0.003858024487 * 3f);
        runPhysics->GearThreeAcceleration = (float)(0.001421377412 * 3f);
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

    // IExtremeGear API Callbacks //
    public RunningProperties GetRunningProperties() => _runningProperties;
}