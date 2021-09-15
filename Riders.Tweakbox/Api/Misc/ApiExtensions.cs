using System;
using Riders.Tweakbox.Interfaces.Structs.Characters;
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

    public unsafe static void MapToNative(this in ApiCharacterParameters parameters, CharacterParameters* native)
    {
        SetValueIfNotNull((CharacterType*)(&native->ShortcutType), parameters.ShortcutType);
        SetValueIfNotNull((CharacterType*)(&native->StatsType), parameters.StatsType);
        SetValueIfNotNull((Gender*)&native->Gender, parameters.Gender);
        SetValueIfNotNull(&native->Height, parameters.Height);
        SetValueIfNotNull(&native->SpeedMultiplier, parameters.SpeedMultiplier);

        SetValueIfNotNull(&native->StatDashLv1, parameters.StatDashLv1);
        SetValueIfNotNull(&native->StatDashLv2, parameters.StatDashLv2);
        SetValueIfNotNull(&native->StatDashLv3, parameters.StatDashLv3);

        SetValueIfNotNull(&native->StatLimitLv1, parameters.StatLimitLv1);
        SetValueIfNotNull(&native->StatLimitLv2, parameters.StatLimitLv2);
        SetValueIfNotNull(&native->StatLimitLv3, parameters.StatLimitLv3);

        SetValueIfNotNull(&native->StatPowerLv1, parameters.StatPowerLv1);
        SetValueIfNotNull(&native->StatPowerLv2, parameters.StatPowerLv2);
        SetValueIfNotNull(&native->StatPowerLv3, parameters.StatPowerLv3);

        SetValueIfNotNull(&native->StatCorneringLv1, parameters.StatCorneringLv1);
        SetValueIfNotNull(&native->StatCorneringLv2, parameters.StatCorneringLv2);
        SetValueIfNotNull(&native->StatCorneringLv3, parameters.StatCorneringLv3);
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

    public unsafe static void SetValueIfNotNull<T>(T* target, in T? value) where T : unmanaged
    {
        if (value.HasValue)
            *target = value.Value;
    }
}
