using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed before the player's air gained from grind for this frame is set.
    /// </summary>
    public static event GenericModifyPlayerFloatFn OnSetAirGainedThisFrameFromGrind;

    /// <summary>
    /// Use this event to override how the player's air gained for this frame from grinding is set.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetAirGainedThisFrameFromGrind;

    /// <summary>
    /// Executed before the player's air gained from fly for this frame is set.
    /// </summary>
    public static event GenericModifyPlayerIntFn OnSetAirGainedThisFrameFromFly;

    /// <summary>
    /// Use this event to override how the player's air gained for this frame from flying is set.
    /// </summary>
    public static event GenericModifyPlayerIntFn SetAirGainedThisFrameFromFly;

    /// <summary>
    /// Executed before the player's air gained from fly for this frame is set.
    /// </summary>
    public static event GenericModifyPlayerIntFn OnSetAirGainedThisFrameFromPower;

    /// <summary>
    /// Use this event to override how the player's air gained for this frame from flying is set.
    /// </summary>
    public static event GenericModifyPlayerIntFn SetAirGainedThisFrameFromPower;

    private static IAsmHook _setAirGainFromGrindHook;
    private static IAsmHook _setAirGainFromFlyHook;
    private static IAsmHook _setAirGainFromPowerHook;

    public void InitSetAirFromTypes(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setAirGainFromGrindHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push esi",
            $"{utilities.PushXmmRegisterFloat("xmm0")}",
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(OnSetGrindAirGainHook), false)}",
            $"{utilities.PopFromX87ToXmm()}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x4E6B42, AsmHookBehaviour.ExecuteFirst).Activate();

        _setAirGainFromFlyHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push edi",
            $"push esi",
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerIntFnPtr>(typeof(EventController), nameof(OnSetFlyAirGainHook), false)}",
            $"mov esi, eax",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x4E75BC, AsmHookBehaviour.ExecuteFirst).Activate();

        _setAirGainFromPowerHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"push ecx\npush edx",  // Register Save
            $"push esi",
            $"push eax",
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerIntFnPtr>(typeof(EventController), nameof(OnSetPowerAirGainHook), false)}",
            $"pop edx\npop ecx",    // Register Restore
        }, 0x4E7C1D, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static float OnSetGrindAirGainHook(float value, Player* player)
    {
        var copy = value;
        OnSetAirGainedThisFrameFromGrind?.Invoke(ref copy, player);
        SetAirGainedThisFrameFromGrind?.Invoke(ref value, player);
        return value;
    }

    [UnmanagedCallersOnly]
    private static int OnSetFlyAirGainHook(int value, Player* player)
    {
        var copy = value;
        OnSetAirGainedThisFrameFromFly?.Invoke(ref copy, player);
        SetAirGainedThisFrameFromFly?.Invoke(ref value, player);
        return value;
    }

    [UnmanagedCallersOnly]
    private static int OnSetPowerAirGainHook(int value, Player* player)
    {
        var copy = value;
        OnSetAirGainedThisFrameFromPower?.Invoke(ref copy, player);
        SetAirGainedThisFrameFromPower(ref value, player);
        return value;
    }
}