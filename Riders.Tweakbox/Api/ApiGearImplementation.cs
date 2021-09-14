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

namespace Riders.Tweakbox.Api;

internal unsafe partial class ApiGearImplementation
{
    private static ApiGearImplementation Instance;

    private CustomGearController _owner;
    private IReloadedHooks _hooks;

    private IHook<Functions.ApplyTurningSpeedLossFnPtr> _applyTurningSpeedLossHook;
    private EventController _eventController;

    private ApiPlayerState[] _playerState;

    public ApiGearImplementation(CustomGearController owner)
    {
        Instance = this;
        _hooks = SDK.ReloadedHooks;
        _owner = owner;
        _owner.OnReset += ResetState;
        _eventController = IoC.GetSingleton<EventController>(); // Ensure load order

        ResetState();
        InitCustomLevels();
        _applyTurningSpeedLossHook = Functions.ApplyTurningSpeedLoss.HookAs<Functions.ApplyTurningSpeedLossFnPtr>(typeof(ApiGearImplementation), nameof(ApplyTurningSpeedLossImplStatic)).Activate();
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
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetHandlingProperties();
            if (props.Enabled)
            {
                var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
                props.SetSpeedLoss.InvokeIfNotNull(ref speedlossproperties->LinearMultiplier, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                speedlossproperties->LinearMultiplier *= props.SpeedLossMultiplier;

            }
        }

        return _applyTurningSpeedLossHook.OriginalFunction.Value.Invoke(player, speedlossproperties).Pointer;
    }

    private unsafe void SetAirGainedThisFrame(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (props.Enabled)
            {
                var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
                
                // Check function override.
                if (props.ShouldGainAir != null && props.ShouldGainAir((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player)) == QueryResult.False)
                    player->AirGainedThisFrame = 0;

                // Check for ring gear override.
                if (!props.GainsRingsOnRingGear)
                    DefaultImplementation();
            }
            else
            {
                DefaultImplementation();
            }
        }
        else
        {
            DefaultImplementation();
        }

        void DefaultImplementation() => player->AirGainedThisFrame *= (player->GearSpecialFlags.HasAllFlags(ExtremeGearSpecialFlags.GearOnRings) ? 0 : 1);
    }

    private void SetAirGainedThisFrameFromGrind(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (props.Enabled)
                value *= props.SpeedAirGainMultiplier.GetValueOrDefault(1.0f);
        }
    }

    private void SetAirGainedThisFrameFromFly(ref int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (props.Enabled)
                value = (int)(value * props.FlyAirGainMultiplier.GetValueOrDefault(1.0f));
        }
    }

    private void SetAirGainedThisFrameFromPower(ref int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (props.Enabled)
                value = (int)(value * props.PowerAirGainMultiplier.GetValueOrDefault(1.0f));

            // Tack on power speed gain.
            var shortcutProps = behaviour.GetShortcutBehaviour();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            float speedGain = 0.0f;
            if (shortcutProps.Enabled)
            {
                shortcutProps.AddPowerShortcutSpeed.InvokeIfNotNull(ref speedGain, (IntPtr) player, playerIndex, GetPlayerLevel(behaviour, player));
                speedGain += shortcutProps.PowerShortcutAddedSpeed;
            }

            player->Speed += speedGain;
        }
    }

    private bool CustomOffroadFunction(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetOffroadProperties();
            if (!props.Enabled)
                return false;

            var ignoreSpeedLoss = props.CheckIfIgnoreSpeedLoss.QueryIfNotNull((IntPtr)player, Sewer56.SonicRiders.API.Player.GetPlayerIndex(player), GetPlayerLevel(behaviour, player));
            if (ignoreSpeedLoss != null && ignoreSpeedLoss.Value.TryConvertToBool(out bool result))
                return result;

            if (props.IgnoreSpeedLoss)
                return true;
        }

        return false;
    }

    private void SetRailSpeedCap(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            
            if (shortcutModifier.Enabled)
            {
                shortcutModifier.SetSpeedShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, index, GetPlayerLevel(behaviour, player));
                value *= shortcutModifier.SpeedShortcutModifier;
            }

            // Mono modifier
            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }
    }

    private Enum<AsmFunctionResult> SetForceLegendEffect(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var legendProperties = behaviour.GetLegendProperties();
            if (!legendProperties.Enabled)
                return AsmFunctionResult.Indeterminate;

            // Query from Mod
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            var result = legendProperties.OverrideLegendEffect.QueryIfNotNull((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
            if (result != QueryResult.Indeterminate)
                return result.Value.ToAsmFunctionResult();

            // Otherwise standard behaviour.
            if (legendProperties.IgnoreOnState.ContainsState((int)player->LastPlayerState))
                return AsmFunctionResult.False;
        }

        return AsmFunctionResult.Indeterminate;
    }

    private void SetFlySpeedX(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (shortcutModifier.Enabled)
            {
                shortcutModifier.SetFlyShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, index, GetPlayerLevel(behaviour, player));
                value *= shortcutModifier.FlyShortcutModifier;
            }

            // Mono modifier
            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }
    }

    private void SetFlySpeedY(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (shortcutModifier.Enabled)
            {
                shortcutModifier.SetFlyShortcutSpeed.InvokeIfNotNull(ref value, (IntPtr)player, index, GetPlayerLevel(behaviour, player));
                value *= shortcutModifier.FlyShortcutModifier;
            }

            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }
    }

    private void HandleDriftBehaviour(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var driftProps = behaviour.GetDriftDashProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (driftProps.Enabled)
            {
                driftProps.SetDriftDashCap.InvokeIfNotNull(ref value, (IntPtr) player, playerIndex, GetPlayerLevel(behaviour, player));
                value = driftProps.DriftDashCap.GetValueOrDefault(value);

                if (driftProps.BoostOnDriftDash)
                    _playerState[playerIndex].DriftBoostedOnLastFrame = true;
            }

            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }
    }

    private Player* AfterSetMovementFlagsOnInput(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            // Boost Properties
            var boostProps = behaviour.GetBoostProperties();
            if (boostProps.Enabled)
                ApplyBoostPropertiesOnInput(player, boostProps, playerIndex, behaviour);

            // Add Berserker
            var berserkerProps = behaviour.GetBerserkerProperties();
            if (berserkerProps.Enabled)
                ApplyBerserkerPropertiesOnInput(player, berserkerProps, playerIndex);

            // Boost on Drift
            var driftProps = behaviour.GetDriftDashProperties();
            if (driftProps.Enabled)
                ApplyDriftPropertiesOnInput(player, playerIndex);

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

    private void ApplyBoostPropertiesOnInput(Player* player, BoostProperties boostProps, int playerIndex, IExtremeGear behaviour)
    {
        // Determine if Remove Boost
        bool cannotBoost = boostProps.CannotBoost.GetValueOrDefault(false);
        var result = boostProps.CheckIfCanBoost.QueryIfNotNull((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
        result.GetValueOrDefault(QueryResult.Indeterminate).TryConvertToBool(out cannotBoost);

        // Remove Boost
        if (cannotBoost && player->PlayerInput->ButtonsPressed.HasAllFlags(Buttons.Decline))
            player->MovementFlags &= ~(MovementFlags.Boosting | MovementFlags.BoostingAirLoss);

        // Set air
        bool hasBoosted = player->MovementFlags.HasAllFlags(MovementFlags.Boosting) && !player->LastMovementFlags.HasAllFlags(MovementFlags.Boosting);
        if (!hasBoosted)
            return;

        boostProps.OnBoost?.Invoke((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
        if (boostProps.AirPercentageOnBoost.HasValue)
        {
            var stats = (&player->LevelOneStats + player->Level);
            player->Air = Math.Min(stats->GearStats.MaxAir, (int)(stats->GearStats.MaxAir * boostProps.AirPercentageOnBoost));
        }
    }

    private void SetBoostChainMultiplier(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var boostProps = behaviour.GetBoostProperties();
            if (boostProps.Enabled)
            {
                var additionalBcm = 0.0f;
                var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

                additionalBcm = boostProps.GetAddedBoostChainMultiplier.QueryIfNotNull((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player)).Value;
                additionalBcm += boostProps.AddedBoostChainMultiplier.GetValueOrDefault(0.0F);
                value += additionalBcm;
            }
        }
    }

    private void SetBoostDuration(ref int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var boostProps = behaviour.GetBoostProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            var addedBoostFrames = 0;

            if (boostProps.Enabled)
            {
                addedBoostFrames += boostProps.GetAddedBoostDuration.QueryIfNotNull((IntPtr) player, playerIndex, GetPlayerLevel(behaviour, player)).Value;
                addedBoostFrames += boostProps.GetExtraBoostDurationForLevel(player->Level);
            }

            value += addedBoostFrames;

            // Update boost duration.
            _playerState[playerIndex].BoostDuration = value;
        }
    }

    private void SetPlayerSpeedOnTrickLand(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var trickProps = behaviour.GetTrickBehaviour();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (trickProps.Enabled)
            {
                trickProps.SetSpeedGain.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                value += trickProps.SpeedGainFlat.GetValueOrDefault(0.0f);
                value *= trickProps.SpeedGainPercentage.GetValueOrDefault(1.0f);
            }
        }
    }

    private void SetExhaustTrailColour(ColorRGBA* value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var exhaustProperties = behaviour.GetExhaustProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (exhaustProperties.Enabled)
            {
                if (exhaustProperties.GetExhaustTrailColour != null)
                {
                    var color = exhaustProperties.GetExhaustTrailColour((IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player), *value);
                    *value = color;
                }
            }
        }
    }

    private void SetTornadoDeceleration(ref float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var tornadoProperties = behaviour.GetTornadoProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (tornadoProperties.Enabled)
            {
                tornadoProperties.SetTornadoSpeed.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                value *= tornadoProperties.SpeedMultiplier.GetValueOrDefault(1.0f);
            }
        }
    }

    private void SetRingCountFromRingPickup(ref int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var itemProps = behaviour.GetItemProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (itemProps.Enabled)
                itemProps.SetRingCountOnPickup.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
        }
    }

    private void SetPitAirGain(ref int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var airProps = behaviour.GetAirProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (airProps.Enabled)
            {
                airProps.SetPitAirGain.InvokeIfNotNull(ref value, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                value = (int)(value * airProps.PitAirGainMultiplier.GetValueOrDefault(1.0f));
            }
        }
    }


    private void SetRunningSpeedHook(Player* player, RunningPhysics2* physics)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var airProps = behaviour.GetRunningProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (airProps.Enabled)
            {
                var intptr = (IntPtr) physics;
                airProps.SetRunningProperties.InvokeIfNotNull(ref intptr, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                physics->GearOneAcceleration += airProps.GearOneAccelerationOffset.GetValueOrDefault(0f);
                physics->GearTwoAcceleration += airProps.GearTwoAccelerationOffset.GetValueOrDefault(0f);
                physics->GearThreeAcceleration += airProps.GearThreeAccelerationOffset.GetValueOrDefault(0f);
                physics->GearOneMaxSpeed += airProps.GearOneMaxSpeedOffset.GetValueOrDefault(0f);
                physics->GearTwoMaxSpeed += airProps.GearTwoMaxSpeedOffset.GetValueOrDefault(0f);
                physics->GearThreeMaxSpeed += airProps.GearThreeMaxSpeedOffset.GetValueOrDefault(0f);
            }
        }
    }

    private Enum<AsmFunctionResult> SetSpeedLossFromWallHit(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Boost Properties
            var wallProps = behaviour.GetWallHitBehaviour();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (wallProps.Enabled)
            {
                var angle = Math.Abs(player->WallBounceAngle);
                var speedLoss = CalcSpeedLoss(angle);

                wallProps.SetSpeedLossOnWallHit.InvokeIfNotNull(ref speedLoss, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                speedLoss *= wallProps.SpeedLossMultiplier.GetValueOrDefault(1.0f);
                speedLoss += wallProps.SpeedGainFlat.GetValueOrDefault(0.0f);
                player->Speed -= speedLoss;

                return AsmFunctionResult.False;
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

        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Boost Properties
            var dashProps = behaviour.GetDashPanelProperties();
            var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (dashProps.Enabled)
            {
                dashProps.SetSpeedGain.InvokeIfNotNull(ref targetSpeed, (IntPtr)player, playerIndex, GetPlayerLevel(behaviour, player));
                targetSpeed += dashProps.AdditionalSpeed.GetValueOrDefault(0.0f);
            }
        }
    }

    private float ApplyMonoShortcutModifier(float value, Player* player, IExtremeGear behaviour)
    {
        // Mono modifier
        var monoTypeModifier = behaviour.GetMonoTypeShortcutBehaviour();
        if (monoTypeModifier.Enabled)
        {
            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            if (IsMonoShortcut((int)player->Character, (int)player->ExtremeGear))
            {
                monoTypeModifier.SetSpeedModifierForMonoType.InvokeIfNotNull(ref value, (IntPtr)player, index, GetPlayerLevel(behaviour, player));
                value *= monoTypeModifier.ExistingTypeSpeedModifierPercent.GetValueOrDefault(1.0f);
            }
            else
            {
                monoTypeModifier.SetSpeedModifierForExistingType.InvokeIfNotNull(ref value, (IntPtr)player, index, GetPlayerLevel(behaviour, player));
                value *= monoTypeModifier.NewTypeSpeedModifierPercent.GetValueOrDefault(1.0f);
            }
        }

        return value;
    }

    private int GetPlayerLevel(IExtremeGear behaviour, Player* player) => behaviour.GetPlayerLevel(player->Level, player->Rings);

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

    private bool TryGetGearBehaviour(int index, out IExtremeGear behaviour)
    {
        if (_owner.TryGetGearData_Internal(index, out var customData) && customData.Behaviour != null)
        {
            behaviour = customData.Behaviour;
            return true;
        }

        behaviour = default;
        return false;
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