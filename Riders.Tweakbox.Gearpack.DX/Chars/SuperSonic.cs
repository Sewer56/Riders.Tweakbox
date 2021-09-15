using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class SuperSonic : CustomCharacterBase, ICustomStats
{
    public override string Name { get; } = "SuperSonic DX";

    public override Characters Character { get; } = Characters.SuperSonic;
}