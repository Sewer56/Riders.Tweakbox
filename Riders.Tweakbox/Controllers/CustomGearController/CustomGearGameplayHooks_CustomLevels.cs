using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.CustomGearController.Behaviour;
using Riders.Tweakbox.Misc.Pointers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using System;
using System.Runtime.InteropServices;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// Handles the custom gear levels part of Custom Gear Gameplay Hooks.
/// </summary>
internal unsafe partial class CustomGearGameplayHooks
{
    private IHook<Functions.PlayerFnPtr> _setStatsForPlayerRaceHook;

    // Base stats (as initialised by game) and real time stats.
    private NativeAllocation<PlayerLevelStats>[] _basePlayerLevelStats    = new NativeAllocation<PlayerLevelStats>[Player.NumberOfPlayers];
    private NativeAllocation<PlayerLevelStats>[] _currentPlayerLevelStats = new NativeAllocation<PlayerLevelStats>[Player.NumberOfPlayers];

    private CustomGearExtendedLevelsPatches _customGearExtendedLevelsPatches;

    // Constructor
    private void InitCustomLevels()
    {
        _customGearExtendedLevelsPatches = new CustomGearExtendedLevelsPatches(this);
        _setStatsForPlayerRaceHook = Functions.SetPlayerStatsForRaceMode.HookAs<Functions.PlayerFnPtr>(typeof(CustomGearGameplayHooks), nameof(OnSetPlayerStatsForRaceModeStatic)).Activate();
    }

    private void InitStats(Player* player, int playerIndex)
    {
        const int DefaultNumLevels = 3;
        int numLevels = DefaultNumLevels;

        // First Reset Stats
        ResetStats(playerIndex);

        // Now allocate memory as needed.
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var stats = behaviour.GetExtendedLevelStats();
            if (stats != null)
                numLevels = stats.Count;
        }

        _basePlayerLevelStats[playerIndex]    = NativeAllocation<PlayerLevelStats>.Create(numLevels);
        _currentPlayerLevelStats[playerIndex] = NativeAllocation<PlayerLevelStats>.Create(numLevels);

        for (int x = 0; x < numLevels; x++)
        {
            _basePlayerLevelStats[playerIndex].Data[x] = default;
            _currentPlayerLevelStats[playerIndex].Data[x] = default;
        }
    }

    private void ResetStats(int playerIndex)
    {
        _basePlayerLevelStats[playerIndex].Free();
        _currentPlayerLevelStats[playerIndex].Free();
    }
    public IntPtr GetDataForIndex(int index) => (IntPtr)_currentPlayerLevelStats[index].Data;

    // Hooks
    private int OnSetPlayerStatsForRaceMode(Player* player)
    {
        // Init Stats.
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        InitStats(player, playerIndex);

        // Note: We will hook the native code to initialise these stats.
        var result = _setStatsForPlayerRaceHook.OriginalFunction.Value.Invoke(player);

        // Copy from base stats to current stats.
        //var playerStatSpan = new Span<PlayerLevelStats>(&player->LevelOneStats, 3);
        //playerStatSpan.CopyTo(_currentPlayerLevelStats[playerIndex].AsSpan());
        _currentPlayerLevelStats[playerIndex].CopyTo(_basePlayerLevelStats[playerIndex]);
        return result;
    }

    #region Static Callbacks
    [UnmanagedCallersOnly]
    private static unsafe int OnSetPlayerStatsForRaceModeStatic(Player* player) => Instance.OnSetPlayerStatsForRaceMode(player);
    #endregion
}
