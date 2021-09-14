using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Api.Misc;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Misc.Pointers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sewer56.SonicRiders.API;
using ExtremeGearLevelStats = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGearLevelStats;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using Riders.Tweakbox.Interfaces.Interfaces;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.Hooks.Utilities.Enums;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Api;

/// <summary>
/// Handles the custom gear levels part of Custom Gear Gameplay Hooks.
/// </summary>
internal unsafe partial class ApiBehaviourImplementation
{
    public static Action OnInitGearStats;

    private const int DefaultNumLevels = 3;

    private IHook<Functions.PlayerFnPtr> _setStatsForPlayerRaceHook;

    // Base stats (as initialised by game) and real time stats.
    private PlayerLevelStats[][] _basePlayerLevelStats = new PlayerLevelStats[Player.NumberOfPlayers][];
    private PlayerLevelStats[][] _currentPlayerLevelStats = new PlayerLevelStats[Player.NumberOfPlayers][];

    // Constructor
    private void InitCustomLevels()
    {
        _setStatsForPlayerRaceHook = Functions.SetPlayerStatsForRaceMode.HookAs<Functions.PlayerFnPtr>(typeof(ApiBehaviourImplementation), nameof(OnSetPlayerStatsForRaceModeStatic)).Activate();
        Sewer56.SonicRiders.API.Event.AfterEndScene += UpdateGearDataAfterRenderFrame;
        EventController.ForceLevelUpHandler += ForceLevelUpHandler;
        EventController.ForceLevelDownHandler += ForceLevelDownHandler;
    }

    private bool InitStats(Player* player, int playerIndex, out Span<ExtendedExtremeGearLevelStats> extendedStats)
    {
        int numLevels = DefaultNumLevels;
        bool result = false;
        extendedStats = default;
        OnInitGearStats?.Invoke();

        // Now allocate memory as needed.
        if (TryGetCustomBehaviour(player, out var behaviours, out _, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var stats = behaviour.GetExtendedLevelStats();
                if (stats != null && stats.ExtendedStats != null)
                {
                    numLevels = stats.ExtendedStats.Length;

                    // Copy gear data.
                    var gearStats = &player->ExtremeGearPtr->GearStatsLevel1;
                    var levels = Math.Min(numLevels, DefaultNumLevels);
                    for (int x = 0; x < levels; x++)
                        gearStats[x] = stats.ExtendedStats[x].ConvertToNative();

                    result = numLevels > DefaultNumLevels;

                    if (result)
                        extendedStats = new Span<ExtendedExtremeGearLevelStats>(stats.ExtendedStats, DefaultNumLevels, numLevels - DefaultNumLevels);
                }

                behaviour.OnReset()?.Invoke((IntPtr)player, playerIndex, level);
            }
        }

        _basePlayerLevelStats[playerIndex]    = new PlayerLevelStats[numLevels];
        _currentPlayerLevelStats[playerIndex] = new PlayerLevelStats[numLevels];
        return result;
    }

    // Hooks
    private int OnSetPlayerStatsForRaceMode(Player* player)
    {
        // Init Stats.
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        if (playerIndex == -1)
            return _setStatsForPlayerRaceHook.OriginalFunction.Value.Invoke(player);

        bool hasExtendedGears = InitStats(player, playerIndex, out var extendedStats);
        ResetState();
        ResetCache();

        // Note: We will hook the native code to initialise these stats.
        var result = _setStatsForPlayerRaceHook.OriginalFunction.Value.Invoke(player);

        // Copy stat data
        var playerStatsSpan = new Span<PlayerLevelStats>(&player->LevelOneStats, DefaultNumLevels);
        var currentPlayerLevelStats = _currentPlayerLevelStats[playerIndex].AsSpan();
        playerStatsSpan.CopyTo(_currentPlayerLevelStats[playerIndex].AsSpan());

        // Calculate extended gears.
        if (hasExtendedGears)
        {
            // Take backup of stats.
            var playerStatsBackup = playerStatsSpan.ToArray();

            // Calculate remaining levels.
            var numLevels = _currentPlayerLevelStats[playerIndex].Length;
            var remainingLevels = numLevels - DefaultNumLevels;
            for (int x = 0; x < remainingLevels; x++)
            {
                player->ExtremeGearPtr->GearStatsLevel3 = extendedStats[x].ConvertToNative();
                _setStatsForPlayerRaceHook.OriginalFunction.Value.Invoke(player); // Recalculate.
                currentPlayerLevelStats[x + DefaultNumLevels] = player->LevelThreeStats;
            }

            // Restore backup
            playerStatsBackup.CopyTo(playerStatsSpan);
        }

        // Copy all stats.
        currentPlayerLevelStats.CopyTo(_basePlayerLevelStats[playerIndex]);

        return result;
    }

    private void UpdateGearDataAfterRenderFrame()
    {
        var numRacers = *State.NumberOfRacers;

        // Calculate Stats.
        for (int x = 0; x < numRacers; x++)
        {
            var playerPtr = &Sewer56.SonicRiders.API.Player.Players.Pointer[x];
            var baseStats = _basePlayerLevelStats[x];
            if (baseStats == null)
                continue;

            var currentStats = _currentPlayerLevelStats[x];
            if (!TryGetCustomBehaviour(playerPtr, out var behaviours, out int playerIndex, out var level))
                continue;

            foreach (var behaviour in behaviours)
            {
                // Update state.
                behaviour.OnFrame()?.Invoke((IntPtr)playerPtr, x, level);

                // Update gear data for all levels.
                for (int levelIndex = 0; levelIndex < baseStats.Length; levelIndex++)
                {
                    var stats = baseStats[levelIndex];
                    UpdateGearData(ref stats, ref Unsafe.AsRef<Player>(playerPtr), behaviour, levelIndex);
                    currentStats[levelIndex] = stats;
                }

                // Copy data to player struct.
                var playerLevelStats = new Span<PlayerLevelStats>(&playerPtr->LevelOneStats, DefaultNumLevels);
                var numStats = Math.Min(DefaultNumLevels, currentStats.Length);
                currentStats.AsSpan(0, numStats).CopyTo(playerLevelStats);

                // Write extended stats.
                var extendedStats = behaviour.GetExtendedLevelStats();
                if (extendedStats != null)
                {
                    // Get Level
                    var existingLevel = _playerState[x].PlayerLevel;
                    var overWriteLevel = Math.Min((byte)(DefaultNumLevels - 1), level);

                    // Check for level up.
                    if (level > existingLevel)
                        _playerState[x].ForceLevelUp = true;
                    else if (level < existingLevel)
                        _playerState[x].ForceLevelDown = true;

                    // Copy stats.
                    playerLevelStats[overWriteLevel] = currentStats[level];

                    // Force up to lv3.
                    playerPtr->Level = (byte) overWriteLevel;
                    _playerState[x].PlayerLevel = (byte) level;
                }
            }
        }
    }

    private void UpdateGearData(ref PlayerLevelStats stats, ref Player player, ICustomStats behaviour, int level)
    {
        var playerPtr = (Player*)Unsafe.AsPointer(ref player);
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerPtr);
        var statsPtr = (PlayerLevelStats*) Unsafe.AsPointer(ref stats);

        // Extended Stats
        var extendedStats = behaviour.GetExtendedLevelStats();
        if (extendedStats != null)
            extendedStats.SetPlayerStats?.Invoke((IntPtr) statsPtr, (IntPtr)playerPtr, playerIndex, level);
        
        // Cruise
        var cruiseProps = behaviour.GetCruisingProperties();
        if (cruiseProps != null)
        {
            var addedSpeed = cruiseProps.TopSpeedPerRing.GetValueOrDefault(0.0f) * player.Rings;
            stats.SpeedCap1 += addedSpeed;
            stats.SpeedCap2 += addedSpeed;
            stats.SpeedCap3 += addedSpeed;
        }

        // Accelerating Boosts & COV-P
        var boostProps = behaviour.GetBoostProperties();
        if (boostProps != null)
        {
            var boostSpeedGain = 0.0f;
            var framesElapsed  = _playerState[playerIndex].FramesSpentBoosting;

            if (boostProps.GetAddedBoostSpeed != null)
                boostSpeedGain += boostProps.GetAddedBoostSpeed((IntPtr) playerPtr, playerIndex, framesElapsed, level);

            boostSpeedGain += boostProps.AddedBoostSpeedFromRingCount.GetValueOrDefault(0.0f) * player.Rings;
            if (boostProps.BoostAcceleration.HasValue && player.BoostFramesLeft > 0)
                boostSpeedGain += framesElapsed * boostProps.BoostAcceleration.Value;

            stats.GearStats.BoostSpeed += boostSpeedGain;
        }
        
        // Berserker
        var berserkProps = behaviour.GetBerserkerProperties();
        if (berserkProps != null)
        {
            if (IsBerserkerMode(playerPtr, berserkProps.TriggerPercentage))
                stats.GearStats.PassiveAirDrain += berserkProps.PassiveDrainIncreaseFlat;
        }
    }

    private Enum<AsmFunctionResult> ForceLevelUpHandler(Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out int playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var stats = behaviour.GetExtendedLevelStats();
                if (stats != null && stats.ExtendedStats != null)
                {
                    var forceLvUp = _playerState[playerIndex].ForceLevelUp;
                    _playerState[playerIndex].ForceLevelUp = false;
                    return forceLvUp;
                }
            }
        }

        return AsmFunctionResult.Indeterminate;
    }
    private Enum<AsmFunctionResult> ForceLevelDownHandler(Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out int playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var stats = behaviour.GetExtendedLevelStats();
                if (stats != null && stats.ExtendedStats != null)
                {
                    var forceLvDown = _playerState[playerIndex].ForceLevelDown;
                    _playerState[playerIndex].ForceLevelDown = false;
                    return forceLvDown;
                }
            }
        }

        return AsmFunctionResult.Indeterminate;
    }

    #region Static Callbacks
    [UnmanagedCallersOnly]
    private static unsafe int OnSetPlayerStatsForRaceModeStatic(Player* player) => Instance.OnSetPlayerStatsForRaceMode(player);
    #endregion
}
