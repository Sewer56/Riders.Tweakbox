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

namespace Riders.Tweakbox.Api;

internal unsafe partial class ApiBehaviourImplementation
{
    private static ApiBehaviourImplementation Instance;

    private CustomGearController _customGearController;
    private CustomCharacterController _customCharacterController;
    private IReloadedHooks _hooks;

    private IHook<Functions.ApplyTurningSpeedLossFnPtr> _applyTurningSpeedLossHook;
    private EventController _eventController;

    private ApiPlayerState[] _playerState;

    public ApiBehaviourImplementation(CustomGearController customGearController, CustomCharacterController customCharacterController)
    {
        Instance = this;
        _hooks = SDK.ReloadedHooks;
        _customGearController = customGearController;
        _customCharacterController = customCharacterController;
        _customGearController.OnReset += ResetState;
        _eventController = IoC.GetSingleton<EventController>(); // Ensure load order

        ResetState();
        InitCustomLevels();
        _applyTurningSpeedLossHook = Functions.ApplyTurningSpeedLoss.HookAs<Functions.ApplyTurningSpeedLossFnPtr>(typeof(ApiBehaviourImplementation), nameof(ApplyTurningSpeedLossImplStatic)).Activate();
        EventController.SetAirGainedThisFrame += SetAirGainedThisFrame;
        EventController.SetAirGainedThisFrameFromGrind += SetAirGainedThisFrameFromGrind;
        EventController.SetAirGainedThisFrameFromFly += SetAirGainedThisFrameFromFly;
        EventController.SetAirGainedThisFrameFromPower += SetAirGainedThisFrameFromPower;
        EventController.HandleCustomOffroadFn += CustomOffroadFunction;
        EventController.SetRailSpeedCap += SetRailSpeedCap;
        EventController.SetFlyRingSpeedX += SetFlySpeedX;
        EventController.SetFlyRingSpeedY += SetFlySpeedY;
        EventController.SetForceLegendEffect += SetForceLegendEffect;
        EventController.SetNewDriftCap += HandleDriftBehaviour;
        EventController.AfterSetMovementFlagsOnInput += AfterSetMovementFlagsOnInput;
        EventController.SetSpeedLossFromWallHit += SetSpeedLossFromWallHit;
        EventController.SetBoostChainMultiplier += SetBoostChainMultiplier;
        EventController.SetBoostDuration += SetBoostDuration;
        EventController.SetPlayerSpeedOnTrickLand += SetPlayerSpeedOnTrickLand;
        EventController.SetDashPanelSpeed += SetDashPanelSpeed;
        EventController.SetExhaustTrailColour += SetExhaustTrailColour;
        EventController.SetTornadoDeceleration += SetTornadoDeceleration;
        EventController.SetRingCountFromRingPickup += SetRingCountFromRingPickup;
        EventController.SetPitAirGain += SetPitAirGain;
        EventController.SetRunningSpeedHook += SetRunningSpeedHook;
        EventController.SetSpeedShoesSpeed += SetSpeedShoesSpeed;
    }

    private void ResetState() => _playerState = new ApiPlayerState[Sewer56.SonicRiders.API.Player.MaxNumberOfPlayers];

    private unsafe Player* ApplyTurningSpeedLossImpl(Player* player, TurningSpeedLossProperties* speedlossproperties)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetHandlingProperties();
                if (props != null)
                {
                    props.SetSpeedLoss.InvokeIfNotNull(ref speedlossproperties->LinearMultiplier, (IntPtr)player, playerIndex, level);
                    speedlossproperties->LinearMultiplier *= props.SpeedLossMultiplier;
                }
            }
        }

        return _applyTurningSpeedLossHook.OriginalFunction.Value.Invoke(player, speedlossproperties).Pointer;
    }

    private unsafe void SetAirGainedThisFrame(Player* player)
    {
        bool? gainsRingsOnRingGear = null;
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetAirProperties();

                if (props == null)
                    continue;

                // Check function override.
                if (props.ShouldGainAir != null && props.ShouldGainAir((IntPtr)player, playerIndex, level) == QueryResult.False)
                    player->AirGainedThisFrame = 0;

                // Check for ring gear override.
                if (props.GainsRingsOnRingGear.HasValue)
                    gainsRingsOnRingGear = props.GainsRingsOnRingGear;
            }
        }

        // Override Ring Gain on Gear
        if (!gainsRingsOnRingGear.HasValue || (gainsRingsOnRingGear.HasValue && !gainsRingsOnRingGear.Value))
            DefaultImplementation();

        void DefaultImplementation() => player->AirGainedThisFrame *= (player->GearSpecialFlags.HasAllFlags(ExtremeGearSpecialFlags.GearOnRings) ? 0 : 1);
    }

    private void SetAirGainedThisFrameFromGrind(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetAirProperties();
                if (props != null)
                    value *= props.SpeedAirGainMultiplier.GetValueOrDefault(1.0f);
            }
        }
    }

    private void SetAirGainedThisFrameFromFly(ref int value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetAirProperties();
                if (props != null)
                    value = (int)(value * props.FlyAirGainMultiplier.GetValueOrDefault(1.0f));
            }
        }
    }

    private void SetAirGainedThisFrameFromPower(ref int value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetAirProperties();
                if (props != null)
                    value = (int)(value * props.PowerAirGainMultiplier.GetValueOrDefault(1.0f));

                // Tack on power speed gain.
                var shortcutProps = behaviour.GetShortcutBehaviour();
                float speedGain = 0.0f;
                if (shortcutProps != null)
                {
                    shortcutProps.AddPowerShortcutSpeed.InvokeIfNotNull(ref speedGain, (IntPtr)player, playerIndex, level);
                    speedGain += shortcutProps.PowerShortcutAddedSpeed;
                }

                player->Speed += speedGain;
            }
        }
    }

    private bool CustomOffroadFunction(Player* player)
    {
        bool? ignoreSpeedLossResult = null;
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                var props = behaviour.GetOffroadProperties();
                if (props == null)
                    continue;

                var ignoreSpeedLoss = props.CheckIfIgnoreSpeedLoss.QueryIfNotNull((IntPtr)player, playerIndex, level);
                if (ignoreSpeedLoss != null && ignoreSpeedLoss.Value.TryConvertToBool(out bool result))
                    return result;

                if (props.IgnoreSpeedLoss.HasValue)
                    ignoreSpeedLossResult = props.IgnoreSpeedLoss.Value;
            }
        }

        if (ignoreSpeedLossResult.HasValue && ignoreSpeedLossResult.Value)
            return true;

        return false;
    }

    private void SetRailSpeedCap(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {            
                // Base Modifier
                var shortcutModifier = behaviour.GetShortcutBehaviour();
                if (shortcutModifier != null)
                {
                    shortcutModifier.SetSpeedShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value *= shortcutModifier.SpeedShortcutModifier;
                }

                // Mono modifier
                value = ApplyMonoShortcutModifier(value, player, playerIndex, behaviour);
            }
        }
    }

    private Enum<AsmFunctionResult> SetForceLegendEffect(Player* player)
    {
        var asmFunctionResult = AsmFunctionResult.Indeterminate;
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {            
                // Base Modifier
                var legendProperties = behaviour.GetLegendProperties();
                if (legendProperties == null)
                    continue;

                // Query from Mod
                var result = legendProperties.OverrideLegendEffect.QueryIfNotNull((IntPtr)player, playerIndex, level);
                if (result != QueryResult.Indeterminate)
                    asmFunctionResult = result.Value.ToAsmFunctionResult();

                // Otherwise standard behaviour.
                if (legendProperties.IgnoreOnState.ContainsState((int)player->LastPlayerState))
                    asmFunctionResult = AsmFunctionResult.False;
            }
        }

        return asmFunctionResult;
    }

    private void SetFlySpeedX(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {            
                // Base Modifier
                var shortcutModifier = behaviour.GetShortcutBehaviour();
                if (shortcutModifier != null)
                {
                    shortcutModifier.SetFlyShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value *= shortcutModifier.FlyShortcutModifier;
                }

                // Mono modifier
                value = ApplyMonoShortcutModifier(value, player, playerIndex, behaviour);
            }
        }
    }

    private void SetFlySpeedY(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var shortcutModifier = behaviour.GetShortcutBehaviour();
                if (shortcutModifier != null)
                {
                    shortcutModifier.SetFlyShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value *= shortcutModifier.FlyShortcutModifier;
                }

                value = ApplyMonoShortcutModifier(value, player, playerIndex, behaviour);
            }
        }
    }

    private void HandleDriftBehaviour(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var driftProps = behaviour.GetDriftDashProperties();
                if (driftProps != null)
                {
                    driftProps.SetDriftDashCap.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value = driftProps.DriftDashCap.GetValueOrDefault(value);

                    if (driftProps.BoostOnDriftDash)
                        _playerState[playerIndex].DriftBoostedOnLastFrame = true;
                }

                value = ApplyMonoShortcutModifier(value, player, playerIndex, behaviour);
            }
        }
    }

    private Player* AfterSetMovementFlagsOnInput(Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Boost Properties
                var boostProps = behaviour.GetBoostProperties();
                if (boostProps != null)
                    ApplyBoostPropertiesOnInput(player, boostProps, playerIndex, level, behaviour);

                // Add Berserker
                var berserkerProps = behaviour.GetBerserkerProperties();
                if (berserkerProps != null)
                    ApplyBerserkerPropertiesOnInput(player, berserkerProps, playerIndex);

                // Boost on Drift
                var driftProps = behaviour.GetDriftDashProperties();
                if (driftProps != null)
                    ApplyDriftPropertiesOnInput(player, playerIndex);
            }

            // Increment Boost Counter
            if (player->MovementFlags.HasAllFlags(MovementFlags.Boosting))
                _playerState[playerIndex].FramesSpentBoosting += 1;
            else
                _playerState[playerIndex].FramesSpentBoosting = 0;
        }

        return player;
    }

    private void ApplyDriftPropertiesOnInput(Player* player, int playerIndex)
    {
        if (!_playerState[playerIndex].DriftBoostedOnLastFrame)
            return;

        player->MovementFlags |= (MovementFlags.Boosting | MovementFlags.BoostingAirLoss);
        _playerState[playerIndex].DriftBoostedOnLastFrame = false;
    }

    private void ApplyBerserkerPropertiesOnInput(Player* player, BerserkerPropertiesDX berserkerProps, int playerIndex)
    {
        if (!IsBerserkerMode(player, berserkerProps.TriggerPercentage))
            return;

        bool lastStateOk = player->LastPlayerState != PlayerState.Grinding && player->LastPlayerState != PlayerState.RotateSection;
        if (player->PlayerState == PlayerState.NormalOnBoard && lastStateOk)
        {
            bool notJumping = !player->MovementFlags.HasAllFlags(MovementFlags.ChargingJump) &&
                              !player->LastMovementFlags.HasAllFlags(MovementFlags.ChargingJump);

            if (notJumping)
            {
                player->MovementFlags |= MovementFlags.Boosting;
                player->BoostFramesLeft = 1;
            }

            var playerLevelStats = &player->LevelOneStats + player->Level;

            // Set min speed.
            if (_playerState[playerIndex].FramesSpentBoosting == 0)
                player->Speed = playerLevelStats->GearStats.BoostSpeed;

            player->Speed *= berserkerProps.SpeedMultiplier;
        }
    }

    private void ApplyBoostPropertiesOnInput(Player* player, BoostProperties boostProps, int playerIndex, int playerLevel, ICustomStats behaviour)
    {
        // Determine if Remove Boost
        bool cannotBoost = boostProps.CannotBoost.GetValueOrDefault(false);
        var result = boostProps.CheckIfCanBoost.QueryIfNotNull((IntPtr)player, playerIndex, playerLevel);
        result.GetValueOrDefault(QueryResult.Indeterminate).TryConvertToBool(out cannotBoost);

        // Remove Boost
        if (cannotBoost && player->PlayerInput->ButtonsPressed.HasAllFlags(Buttons.Decline))
            player->MovementFlags &= ~(MovementFlags.Boosting | MovementFlags.BoostingAirLoss);

        // Set air
        bool hasBoosted = player->MovementFlags.HasAllFlags(MovementFlags.Boosting) && !player->LastMovementFlags.HasAllFlags(MovementFlags.Boosting);
        if (!hasBoosted)
            return;

        boostProps.OnBoost?.Invoke((IntPtr)player, playerIndex, playerLevel);
        if (boostProps.AirPercentageOnBoost.HasValue)
        {
            var stats = (&player->LevelOneStats + player->Level);
            player->Air = Math.Min(stats->GearStats.MaxAir, (int)(stats->GearStats.MaxAir * boostProps.AirPercentageOnBoost));
        }
    }

    private void SetBoostChainMultiplier(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var boostProps = behaviour.GetBoostProperties();
                if (boostProps != null)
                {
                    var additionalBcm = 0.0f;
                    additionalBcm = boostProps.GetAddedBoostChainMultiplier.QueryIfNotNull((IntPtr)player, playerIndex, level).Value;
                    additionalBcm += boostProps.AddedBoostChainMultiplier.GetValueOrDefault(0.0F);
                    value += additionalBcm;
                }
            }
        }
    }

    private void SetBoostDuration(ref int value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var boostProps = behaviour.GetBoostProperties();
                var addedBoostFrames = 0;

                if (boostProps != null)
                {
                    addedBoostFrames += boostProps.GetAddedBoostDuration.QueryIfNotNull((IntPtr)player, playerIndex, level).Value;
                    addedBoostFrames += boostProps.GetExtraBoostDurationForLevel(player->Level);
                }

                value += addedBoostFrames;

                // Update boost duration.
                _playerState[playerIndex].BoostDuration = value;
            }
        }
    }

    private void SetPlayerSpeedOnTrickLand(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var trickProps = behaviour.GetTrickBehaviour();
                if (trickProps != null)
                {
                    trickProps.SetSpeedGain.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value += trickProps.SpeedGainFlat.GetValueOrDefault(0.0f);
                    value *= trickProps.SpeedGainPercentage.GetValueOrDefault(1.0f);
                }
            }
        }
    }

    private void SetExhaustTrailColour(ColorRGBA* value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var exhaustProperties = behaviour.GetExhaustProperties();
                if (exhaustProperties != null)
                {
                    if (exhaustProperties.GetExhaustTrailColour != null)
                    {
                        var color = exhaustProperties.GetExhaustTrailColour((IntPtr)player, playerIndex, level, *value);
                        *value = color;
                    }
                }
            }
        }
    }

    private void SetTornadoDeceleration(ref float value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var tornadoProperties = behaviour.GetTornadoProperties();
                if (tornadoProperties != null)
                {
                    tornadoProperties.SetTornadoSpeed.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value *= tornadoProperties.SpeedMultiplier.GetValueOrDefault(1.0f);
                }
            }
        }
    }

    private void SetRingCountFromRingPickup(ref int value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var itemProps = behaviour.GetItemProperties();
                if (itemProps != null)
                    itemProps.SetRingCountOnPickup.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
            }
        }
    }

    private void SetPitAirGain(ref int value, Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var airProps = behaviour.GetAirProperties();
                if (airProps != null)
                {
                    airProps.SetPitAirGain.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, level);
                    value = (int)(value * airProps.PitAirGainMultiplier.GetValueOrDefault(1.0f));
                }
            }
        }
    }


    private void SetRunningSpeedHook(Player* player, RunningPhysics2* physics)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Base Modifier
                var airProps = behaviour.GetRunningProperties();
                if (airProps != null)
                {
                    var intptr = (IntPtr)physics;
                    airProps.SetRunningProperties.InvokeIfNotNull(ref intptr, (IntPtr)player, playerIndex, level);
                    physics->GearOneAcceleration += airProps.GearOneAccelerationOffset.GetValueOrDefault(0f);
                    physics->GearTwoAcceleration += airProps.GearTwoAccelerationOffset.GetValueOrDefault(0f);
                    physics->GearThreeAcceleration += airProps.GearThreeAccelerationOffset.GetValueOrDefault(0f);
                    physics->GearOneMaxSpeed += airProps.GearOneMaxSpeedOffset.GetValueOrDefault(0f);
                    physics->GearTwoMaxSpeed += airProps.GearTwoMaxSpeedOffset.GetValueOrDefault(0f);
                    physics->GearThreeMaxSpeed += airProps.GearThreeMaxSpeedOffset.GetValueOrDefault(0f);
                }
            }
        }
    }

    private Enum<AsmFunctionResult> SetSpeedLossFromWallHit(Player* player)
    {
        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Boost Properties
                var wallProps = behaviour.GetWallHitBehaviour();
                if (wallProps != null)
                {
                    var angle = Math.Abs(player->WallBounceAngle);
                    var speedLoss = CalcSpeedLoss(angle);

                    wallProps.SetSpeedLossOnWallHit.InvokeIfNotNull(ref speedLoss, (IntPtr)player, playerIndex, level);
                    speedLoss *= wallProps.SpeedLossMultiplier.GetValueOrDefault(1.0f);
                    speedLoss += wallProps.SpeedGainFlat.GetValueOrDefault(0.0f);
                    player->Speed -= speedLoss;

                    return AsmFunctionResult.False;
                }
            }
        }

        return AsmFunctionResult.Indeterminate;

        const float scaledSpeedLoss = 0.046296295f;
        float CalcSpeedLoss(float angle) => (float)(angle / (Math.PI / 2.0f) * scaledSpeedLoss);
    }

    private unsafe void SetSpeedShoesSpeed(Player* player, ref float targetSpeed)
    {
        ref var props = ref Static.SpeedShoeProperties;

        targetSpeed = props.Mode switch
        {
            SpeedShoesMode.Vanilla => targetSpeed,
            SpeedShoesMode.Fixed => props.FixedSpeed,
            SpeedShoesMode.Additive => player->Speed + props.AdditiveSpeed,
            SpeedShoesMode.Multiplicative => player->Speed * (1 + props.MultiplicativeSpeed),
            SpeedShoesMode.MultiplyOrFixed => Math.Max(player->Speed * (1 + props.MultiplicativeSpeed), props.MultiplicativeMinSpeed),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private unsafe void SetDashPanelSpeed(Player* player, ref float targetSpeed)
    {
        ref var props = ref Static.PanelProperties;

        targetSpeed = props.Mode switch
        {
            DashPanelMode.Vanilla => targetSpeed,
            DashPanelMode.Fixed => props.FixedSpeed,
            DashPanelMode.Additive => player->Speed + props.AdditiveSpeed,
            DashPanelMode.Multiplicative => player->Speed * (1 + props.MultiplicativeSpeed),
            DashPanelMode.MultiplyOrFixed => Math.Max(player->Speed * (1 + props.MultiplicativeSpeed), props.MultiplicativeMinSpeed)
        };

        if (TryGetCustomBehaviour(player, out var behaviours, out var playerIndex, out var level))
        {
            foreach (var behaviour in behaviours)
            {
                // Boost Properties
                var dashProps = behaviour.GetDashPanelProperties();
                if (dashProps != null)
                {
                    dashProps.SetSpeedGain.InvokeIfNotNull(ref targetSpeed, (IntPtr)player, playerIndex, level);
                    targetSpeed += dashProps.AdditionalSpeed.GetValueOrDefault(0.0f);
                }
            }
        }
    }

    private float ApplyMonoShortcutModifier(float value, Player* player, int playerIndex, ICustomStats behaviour)
    {
        // Mono modifier
        var monoTypeModifier = behaviour.GetMonoTypeShortcutBehaviour();
        if (monoTypeModifier != null)
        {
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (IsMonoShortcut((int)player->Character, (int)player->ExtremeGear))
            {
                monoTypeModifier.SetSpeedModifierForMonoType.InvokeIfNotNull(ref value, (IntPtr)player, index, playerIndex);
                value *= monoTypeModifier.ExistingTypeSpeedModifierPercent.GetValueOrDefault(1.0f);
            }
            else
            {
                monoTypeModifier.SetSpeedModifierForExistingType.InvokeIfNotNull(ref value, (IntPtr)player, index, playerIndex);
                value *= monoTypeModifier.NewTypeSpeedModifierPercent.GetValueOrDefault(1.0f);
            }
        }

        return value;
    }

    private int GetPlayerLevel(List<ICustomStats> behaviours, Player* player)
    {
        int? maxLevel = null;
        foreach (var behaviour in behaviours)
        {
            var level = GetPlayerLevel(behaviour, player);
            if (level.HasValue)
            {
                if (maxLevel == null)
                    maxLevel = level;
                else if (level.Value > maxLevel.Value)
                    maxLevel = level.Value;
            }
        }

        if (maxLevel != null)
            return maxLevel.Value;

        return player->Level;
    }

    private int? GetPlayerLevel(ICustomStats behaviour, Player* player) => behaviour.TryGetPlayerLevel(player->Rings);

    private bool IsMonoShortcut(int character, int gearIndex)
    {
        ref var characterParameter = ref Sewer56.SonicRiders.API.Player.CharacterParameters[character];
        var gearPtr = (ExtremeGear*)Sewer56.SonicRiders.API.Player.Gears.GetPointerToElement(gearIndex);
        return gearPtr->ExtraTypes.ContainsType((FormationTypes)characterParameter.ShortcutType);
    }

    private bool IsBerserkerMode(Player* player, float triggerPercentage)
    {
        var stats = &player->LevelOneStats + player->Level;
        var airNeeded = triggerPercentage * stats->GearStats.MaxAir;
        return player->Air > airNeeded;
    }

    #region Static Callbacks
    [UnmanagedCallersOnly]
    private static unsafe Player* ApplyTurningSpeedLossImplStatic(Player* player, TurningSpeedLossProperties* speedlossproperties) => Instance.ApplyTurningSpeedLossImpl(player, speedlossproperties);
    #endregion
}
public struct ApiPlayerState
{
    public bool DriftBoostedOnLastFrame;
    public byte PlayerLevel;
    public bool ForceLevelUp;
    public bool ForceLevelDown;
    public int BoostDuration;
    public int FramesSpentBoosting;
}