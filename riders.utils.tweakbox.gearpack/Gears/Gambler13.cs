using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class Gambler13 : CustomGearBase, IExtremeGear
{
    private ItemProperties _itemProperties;
    private BoostProperties _boostProperties;

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        _itemProperties = new ItemProperties()
        {
            Enabled = true,
            SetRingCountOnPickup = SetRingCountOnPickup
        };

        _boostProperties = new BoostProperties()
        {
            Enabled = true,
            GetAddedBoostSpeed = GetAddedBoostSpeed,
            OnBoost = OnBoost,
            GetAddedBoostChainMultiplier = GetAddedBoostChainMultiplier
        };

        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Gambler 1.3"));
        data.Behaviour = this;
        gearApi.AddGear(data);
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

    // API Callbacks.
    public ItemProperties GetItemProperties() => _itemProperties;
    public BoostProperties GetBoostProperties() => _boostProperties;
}
