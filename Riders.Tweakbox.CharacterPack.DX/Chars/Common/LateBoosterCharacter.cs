using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class LateBoosterCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDurationLv2 = 60,
        AddedBoostDurationLv3 = 60,
    };

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplierOffset = 0f
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
}
