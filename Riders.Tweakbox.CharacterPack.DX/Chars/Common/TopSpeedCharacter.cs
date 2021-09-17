using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class TopSpeedCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDurationLv1 = 40,
        AddedBoostDurationLv2 = 40,
        AddedBoostDurationLv3 = 40
    };

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplier = 1.05f
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
}
