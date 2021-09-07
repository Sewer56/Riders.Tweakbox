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

namespace Riders.Tweakbox.Controllers.CustomGearController;

internal unsafe partial class CustomGearGameplayHooks
{
    private static CustomGearGameplayHooks Instance;

    private CustomGearController _owner;
    private IReloadedHooks _hooks;

    private IHook<Functions.ApplyTurningSpeedLossFnPtr> _applyTurningSpeedLossHook;
    private EventController _eventController;

    private PersistedState[] _state;

    public CustomGearGameplayHooks(CustomGearController owner)
    {
        Instance = this;
        _hooks = SDK.ReloadedHooks;
        _owner = owner;
        _owner.OnReset += ResetState;
        _eventController = IoC.GetSingleton<EventController>(); // Ensure load order

        ResetState();
        InitCustomLevels();
        _applyTurningSpeedLossHook = Functions.ApplyTurningSpeedLoss.HookAs<Functions.ApplyTurningSpeedLossFnPtr>(typeof(CustomGearGameplayHooks), nameof(ApplyTurningSpeedLossImplStatic)).Activate();
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
    }

    private void ResetState() => _state = new PersistedState[CustomGearCodePatcher.MaxGearCount];

    private unsafe Player* ApplyTurningSpeedLossImpl(Player* player, TurningSpeedLossProperties* speedlossproperties)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetHandlingProperties();
            if (props.Enabled)
                speedlossproperties->LinearMultiplier *= props.SpeedLossMultiplier;
        }

        return _applyTurningSpeedLossHook.OriginalFunction.Value.Invoke(player, speedlossproperties).Pointer;
    }

    private unsafe void SetAirGainedThisFrame(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (props.Enabled && !props.GainsRingsOnRingGear)
                DefaultImplementation();
        }
        else
            DefaultImplementation();

        void DefaultImplementation() => player->AirGainedThisFrame *= (player->GearSpecialFlags.HasAllFlags(ExtremeGearSpecialFlags.GearOnRings) ? 0 : 1);
    }

    private float SetAirGainedThisFrameFromGrind(float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (!props.Enabled)
                return value;

            return value * props.SpeedAirGain;
        }

        return value;
    }

    private int SetAirGainedThisFrameFromFly(int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (!props.Enabled)
                return value;

            return (int)(value * props.FlyAirGain);
        }

        return value;
    }

    private int SetAirGainedThisFrameFromPower(int value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetAirProperties();
            if (!props.Enabled)
                return value;

            return (int)(value * props.PowerAirGain);
        }

        return value;
    }

    private bool CustomOffroadFunction(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            var props = behaviour.GetOffroadProperties();
            if (!props.Enabled)
                return false;

            if (props.IgnoreSpeedLoss)
                return true;
        }

        return false;
    }

    private float SetRailSpeedCap(float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            if (shortcutModifier.Enabled)
                value *= shortcutModifier.SpeedShortcutModifier;

            // Mono modifier
            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }

        return value;
    }

    private Enum<AsmFunctionResult> SetForceLegendEffect(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var legendProperties = behaviour.GetLegendProperties();
            if (legendProperties.Enabled)
            {
                if (legendProperties.IgnoreOnState.ContainsState((int) player->LastPlayerState))
                    return AsmFunctionResult.False;
            }
        }

        return AsmFunctionResult.Indeterminate;
    }

    private float SetFlySpeedX(float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            if (shortcutModifier.Enabled)
                value *= shortcutModifier.FlyShortcutModifier;

            // Mono modifier
            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }

        return value;
    }

    private float SetFlySpeedY(float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var shortcutModifier = behaviour.GetShortcutBehaviour();
            if (shortcutModifier.Enabled)
                value *= shortcutModifier.FlyShortcutModifier;

            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }

        return value;
    }


    private float HandleDriftBehaviour(float value, Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Base Modifier
            var driftProps = behaviour.GetDriftDashProperties();
            if (driftProps.Enabled)
            {
                if (driftProps.DriftDashCap != null)
                    value = driftProps.DriftDashCap.Value;

                if (driftProps.BoostOnDriftDash)
                    _state[(int)player->ExtremeGear].DriftBoostedOnLastFrame = true;
            }

            value = ApplyMonoShortcutModifier(value, player, behaviour);
        }

        return value;
    }

    private Player* AfterSetMovementFlagsOnInput(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Boost Properties
            var boostProps = behaviour.GetBoostProperties();
            if (boostProps.Enabled)
            {
                if (player->PlayerInput->ButtonsPressed.HasAllFlags(Buttons.Decline) && boostProps.CannotBoost)
                    player->MovementFlags &= ~(MovementFlags.Boosting | MovementFlags.BoostingAirLoss);
            }

            // Base Modifier
            var driftProps = behaviour.GetDriftDashProperties();
            if (driftProps.Enabled && _state[(int)player->ExtremeGear].DriftBoostedOnLastFrame)
            {
                player->MovementFlags |= (MovementFlags.Boosting | MovementFlags.BoostingAirLoss);
                _state[(int)player->ExtremeGear].DriftBoostedOnLastFrame = false;
            }
        }

        return player;
    }

    private Enum<AsmFunctionResult> SetSpeedLossFromWallHit(Player* player)
    {
        if (TryGetGearBehaviour((int)player->ExtremeGear, out var behaviour))
        {
            // Boost Properties
            var wallProps = behaviour.GetWallHitBehaviour();
            if (wallProps.Enabled)
            {
                var angle     = Math.Abs(player->WallBounceAngle);
                var speedLoss = CalcSpeedLoss(angle);

                if (wallProps.SpeedLossMultiplier.HasValue)
                    player->Speed -= CalcSpeedLoss(angle) * wallProps.SpeedLossMultiplier.Value;
                else
                    player->Speed -= CalcSpeedLoss(angle);

                if (wallProps.SpeedGainFlat.HasValue)
                    player->Speed += wallProps.SpeedGainFlat.Value;
                
                return AsmFunctionResult.False;
            }
        }

        return AsmFunctionResult.Indeterminate;

        const float scaledSpeedLoss = 0.046296295f;
        float CalcSpeedLoss(float angle) => (float) (angle / (Math.PI / 2.0f) * scaledSpeedLoss);
    }

    private float ApplyMonoShortcutModifier(float value, Player* player, IExtremeGear behaviour)
    {
        // Mono modifier
        var monoTypeModifier = behaviour.GetMonoTypeShortcutBehaviour();
        if (monoTypeModifier.Enabled)
        {
            if (IsMonoShortcut((int)player->Character, (int)player->ExtremeGear))
                value *= monoTypeModifier.ExistingTypeSpeedModifierPercent;
            else
                value *= monoTypeModifier.NewTypeSpeedModifierPercent;
        }

        return value;
    }

    private bool IsMonoShortcut(int character, int gearIndex)
    {
        ref var characterParameter = ref Sewer56.SonicRiders.API.Player.CharacterParameters[character];
        var gearPtr = (ExtremeGear*)Sewer56.SonicRiders.API.Player.Gears.GetPointerToElement(gearIndex);
        return gearPtr->ExtraTypes.ContainsType((FormationTypes)characterParameter.ShortcutType);
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

    public struct PersistedState
    {
        public bool DriftBoostedOnLastFrame;
    }

    #region Static Callbacks
    [UnmanagedCallersOnly]
    private static unsafe Player* ApplyTurningSpeedLossImplStatic(Player* player, TurningSpeedLossProperties* speedlossproperties) => Instance.ApplyTurningSpeedLossImpl(player, speedlossproperties);
    #endregion
}
