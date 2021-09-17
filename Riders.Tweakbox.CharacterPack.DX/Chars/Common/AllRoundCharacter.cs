using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class AllRoundCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDurationLv1 = 20,
        AddedBoostDurationLv2 = 20,
        AddedBoostDurationLv3 = 20,
        AddedBoostChainMultiplier = 0.015f
    };

    private DriftDashProperties _dashProperties = new DriftDashProperties()
    {
        SetDriftDashCap = SetDriftDashCap
    };

    private ExtendedLevelStats _extendedLevelStats = new ExtendedLevelStats()
    {
        SetPlayerStats = SetPlayerStats
    };

    private unsafe static void SetPlayerStats(IntPtr levelstatsptr, IntPtr playerptr, int playerindex, int playerlevel)
    {
        var stats = (Sewer56.SonicRiders.Structures.Gameplay.PlayerLevelStats*)(levelstatsptr);
        stats->GearStats.SpeedGainedFromDriftDash += Utility.SpeedometerToFloat(10);
    }

    protected static float SetDriftDashCap(IntPtr playerptr, int playerindex, int playerlevel, float value) => value + Utility.SpeedometerToFloat(10);

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplier = 1.025f
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
    public ExtendedLevelStats GetExtendedLevelStats() => _extendedLevelStats;
    public DriftDashProperties GetDriftDashProperties() => _dashProperties;
}
