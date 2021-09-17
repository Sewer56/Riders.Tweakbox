using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class NullCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDurationLv1 = 70,
        AddedBoostDurationLv2 = 70,
        AddedBoostDurationLv3 = 70,
        AddedBoostChainMultiplier = 0.03f
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
        stats->GearStats.SpeedGainedFromDriftDash += Utility.SpeedometerToFloat(20);
        stats->GearStats.BoostSpeed += Utility.SpeedometerToFloat(5);
    }

    protected static float SetDriftDashCap(IntPtr playerptr, int playerindex, int playerlevel, float value) => value + Utility.SpeedometerToFloat(20);

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplier = 1.068f,
        ShortcutType = (CharacterType?) 3,
        StatsType = CharacterType.Speed
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
    public ExtendedLevelStats GetExtendedLevelStats() => _extendedLevelStats;
    public DriftDashProperties GetDriftDashProperties() => _dashProperties;
}
