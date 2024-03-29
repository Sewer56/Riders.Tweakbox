﻿using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class TopSpeedCharacter : CustomCharacterBase, ICustomCharacter
{
    private BoostProperties _boostProperties = new BoostProperties()
    {
        AddedBoostDuration = new[] { 40, 40, 40 },
    };

    public ApiCharacterParameters GetCharacterParameters() => new ApiCharacterParameters()
    {
        SpeedMultiplierOffset = 0.05f
    };

    public BoostProperties GetBoostProperties() => _boostProperties;
}
