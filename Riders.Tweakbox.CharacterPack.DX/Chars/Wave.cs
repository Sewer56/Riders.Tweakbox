﻿using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars;

public class Wave : TopSpeedCharacter, ICustomCharacter
{
    public override string Name { get; } = "Wave DX";

    public override Characters Character { get; } = Characters.Wave;
}