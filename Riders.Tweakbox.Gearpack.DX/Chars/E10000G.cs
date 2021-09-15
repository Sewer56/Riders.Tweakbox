using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class E10000G : NullCharacter, ICustomStats
{
    public override string Name { get; } = "E10000G DX";

    public override Characters Character { get; } = Characters.E10000G;
}