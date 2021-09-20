using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class Nights : AllRoundCharacter, ICustomCharacter
{
    public override string Name { get; } = "Nights DX";

    public override Characters Character { get; } = Characters.Nights;
}