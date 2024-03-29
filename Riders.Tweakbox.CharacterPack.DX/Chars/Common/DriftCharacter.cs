﻿using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class DriftCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDuration = new[] { 30, 15, 0 }
    };

    private DriftDashProperties _dashProperties = new DriftDashProperties()
    {
        SetDriftDashCap = SetDriftDashCap
    };

    private ExtendedLevelStats _extendedLevelStats = new ExtendedLevelStats()
    {
        SetPlayerStats = SetPlayerStats
    };

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplierOffset = 0f
    };

    private static unsafe void SetPlayerStats(IntPtr levelstatsptr, IntPtr playerptr, int playerindex, int playerlevel)
    {
        var stats = (Sewer56.SonicRiders.Structures.Gameplay.PlayerLevelStats*)(levelstatsptr);
        stats->GearStats.SpeedGainedFromDriftDash += Utility.SpeedometerToFloat(20);
    }

    protected static float SetDriftDashCap(IntPtr playerptr, int playerindex, int playerlevel, float value) => value + Utility.SpeedometerToFloat(20);

    public BoostProperties GetBoostProperties() => _boostProperties;
    public ExtendedLevelStats GetExtendedLevelStats() => _extendedLevelStats;
    public DriftDashProperties GetDriftDashProperties() => _dashProperties;
}
