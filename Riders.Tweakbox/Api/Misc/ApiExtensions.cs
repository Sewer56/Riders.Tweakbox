using System;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Api.Misc;

public static class ApiExtensions
{
    public static ExtremeGearLevelStats ConvertToNative(this in ExtendedExtremeGearLevelStats stats)
    {
        return new ExtremeGearLevelStats()
        {
            BoostSpeed = stats.BoostSpeed,
            BoostCost = stats.BoostCost,
            DriftAirCost = stats.DriftAirCost,
            MaxAir = stats.MaxAir,
            PassiveAirDrain = stats.PassiveAirDrain,
            SpeedGainedFromDriftDash = stats.SpeedGainedFromDriftDash,
            TornadoCost = stats.TornadoCost
        };
    }

    public static AsmFunctionResult ToAsmFunctionResult(this QueryResult result)
    {
        return result switch
        {
            QueryResult.False => AsmFunctionResult.False,
            QueryResult.True => AsmFunctionResult.True,
            QueryResult.Indeterminate => AsmFunctionResult.Indeterminate,
            _ => AsmFunctionResult.Indeterminate
        };
    }
}
