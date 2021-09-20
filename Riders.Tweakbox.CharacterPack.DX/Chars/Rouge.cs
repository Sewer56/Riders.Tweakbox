using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class Rouge : DriftCharacter, ICustomCharacter
{
    public override string Name { get; } = "Rouge DX";

    public override Characters Character { get; } = Characters.Rouge;
}