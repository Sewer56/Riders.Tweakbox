using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class Eggman : AllRoundCharacter, ICustomStats
{
    public override string Name { get; } = "Eggman DX";

    public override Characters Character { get; } = Characters.Robotnik;
}