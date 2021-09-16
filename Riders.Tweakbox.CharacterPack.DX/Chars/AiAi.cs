using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class AiAi : LateBoosterCharacter, ICustomStats
{
    public override string Name { get; } = "Saru DX"; // Monke!

    public override Characters Character { get; } = Characters.AiAi;
}