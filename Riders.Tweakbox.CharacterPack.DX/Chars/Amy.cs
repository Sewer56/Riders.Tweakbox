using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class Amy : DriftCharacter, ICustomCharacter
{
    public override string Name { get; } = "Amy DX";

    public override Characters Character { get; } = Characters.Amy;
}