using System;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;
using EnumsNET;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using Riders.Tweakbox.Interfaces.Interfaces;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.Hooks.Utilities.Enums;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Sewer56.SonicRiders.Structures.Input.Enums;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Misc.Pointers;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc.Types;
using Riders.Tweakbox.Interfaces.Internal;
using Riders.Tweakbox.Interfaces.Structs;
using Riders.Tweakbox.Api.Misc;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.SonicRiders.Structures.Misc;
using Riders.Tweakbox.Controllers.CustomCharacterController;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Riders.Tweakbox.Api;

internal unsafe partial class ApiBehaviourImplementation
{
    private BehaviourCacheData[] _behaviourCache = new BehaviourCacheData[Sewer56.SonicRiders.API.Player.MaxNumberOfPlayers];

    private void ResetCache()
    {
        for (var x = 0; x < _behaviourCache.Length; x++)
            _behaviourCache[x] = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveOptimization)]
    private bool TryGetCustomBehaviour(Player* player, out List<ICustomStats> behaviours, out int playerIndex, out int playerLevel)
    {
        playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

        // Cached read: Subsequent times since initial stat calculation.
        if (TryGetCustomBehaviourFromCache(playerIndex, out behaviours, out playerLevel))
            return true;

        // Non cached read (1st time every race)
        int gearIndex = (int)player->ExtremeGear;
        behaviours = new List<ICustomStats>();

        if (_customGearController.TryGetGearData_Internal(gearIndex, out var customData) && customData.Behaviour != null)
            behaviours.Add(customData.Behaviour);

        int characterIndex = (int)player->Character;
        if (_customCharacterController.TryGetCharacterBehaviours_Internal(characterIndex, out var charRequests))
        {
            foreach (var charBehaviour in charRequests)
                behaviours.Add(charBehaviour.Behaviour);
        }

        if (behaviours.Count <= 0)
        {
            playerIndex = -1;
            playerLevel = -1;
        }
        else
        {
            playerLevel = GetPlayerLevel(behaviours, player);
        }

        return behaviours.Count > 0;
    }

    private bool TryGetCustomBehaviourFromCache(int playerIndex, out List<ICustomStats> behaviours, out int playerLevel)
    {
        var cache = _behaviourCache[playerIndex];
        if (cache.Behaviours != null)
        {
            behaviours  = cache.Behaviours;
            playerLevel = cache.PlayerLevel;
            return true;
        }
        else
        {
            behaviours = default;
            playerLevel = default;
            return false;
        }
    }

    private struct BehaviourCacheData
    {
        public List<ICustomStats> Behaviours;
        public int PlayerLevel;
    }
}