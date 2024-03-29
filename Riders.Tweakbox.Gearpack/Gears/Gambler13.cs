﻿using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Gambler13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Gambler 1.3";

    private ItemProperties _itemProperties;
    private BoostProperties _boostProperties;

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void InitializeGear(string gearsFolder, ICustomGearApi gearApi)
    {
        _itemProperties = new ItemProperties()
        {
            SetRingCountOnPickup = SetRingCountOnPickup
        };

        _boostProperties = new BoostProperties()
        {
            GetAddedBoostSpeed = GetAddedBoostSpeed,
            OnBoost = OnBoost,
            GetAddedBoostChainMultiplier = GetAddedBoostChainMultiplier
        };
    }

    private unsafe float GetAddedBoostChainMultiplier(IntPtr playerPtr, int playerIndex, int level)
    {
        var player = (Player*)playerPtr;
        if (player->Rings >= 90)
            return 0.05f;

        return 0;
    }

    private unsafe void OnBoost(IntPtr playerPtr, int playerIndex, int level)
    {
        var player = (Player*)playerPtr;

        if (player->Level >= 1)
            player->Rings -= 10;
    }

    private unsafe float GetAddedBoostSpeed(IntPtr playerPtr, int playerIndex, int framesBoosting, int level)
    {
        var player = (Player*)playerPtr;
        if (player->Rings > 90)
            return Utility.SpeedometerToFloat(55);

        return 0;
    }

    private int SetRingCountOnPickup(IntPtr playerPtr, int playerIndex, int playerLevel, int value) => value + 1;

    // IExtremeGear API Callbacks //
    public ItemProperties GetItemProperties() => _itemProperties;
    public BoostProperties GetBoostProperties() => _boostProperties;
}
