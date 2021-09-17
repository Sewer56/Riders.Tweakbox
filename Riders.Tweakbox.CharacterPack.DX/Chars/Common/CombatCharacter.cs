using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class CombatCharacter : CustomCharacterBase, ICustomStats
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostChainMultiplier = 0.03f,
        AddedBoostDurationLv1 = 70,
        AddedBoostDurationLv2 = 70,
    };

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplierOffset = 0f
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
}
