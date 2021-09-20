using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using CharacterType = Riders.Tweakbox.Interfaces.Structs.Characters.CharacterType;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class E10000R : DriftCharacter, ICustomCharacter
{
    public override string Name { get; } = "E10000R DX";

    public override Characters Character { get; } = Characters.E10000R;

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplierOffset = 0f,
        ShortcutType = CharacterType.Power
    };
}