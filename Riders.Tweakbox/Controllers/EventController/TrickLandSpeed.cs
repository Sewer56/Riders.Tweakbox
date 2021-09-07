using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed as the player speed on trick land is set.
    /// </summary>
    public static GenericModifyPlayerFloatFn OnSetPlayerSpeedOnTrickLand;

    /// <summary>
    /// Overrides the speed set on trick land for the player.
    /// </summary>
    public static GenericModifyPlayerFloatFn SetPlayerSpeedOnTrickLand;

    private IAsmHook _setTrickSpeedFromOtherRamp;
    private IAsmHook _setTrickSpeedFromManualRamp;

    public void InitTrickLandSpeed(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {

        _setTrickSpeedFromManualRamp = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            "push esi", // Push Player
            "push dword [ecx*4 + 0x5C4480]", // Speed
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(ModifyTrickLandSpeed), false)}",
            $"{utilities.PopFromX87ToXmm()}",
            $"{utilities.PopCdeclCallerSavedRegisters()}"

        }, 0x4E955C, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        _setTrickSpeedFromOtherRamp = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegistersExcept("ecx")}",
            "push esi", // Push Player
            "push dword [edx*4+0x5C4410]", // Speed
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(ModifyTrickLandSpeed), false)}",
            $"{utilities.PopFromX87ToRegister("ecx")}",
            $"{utilities.PopCdeclCallerSavedRegistersExcept("ecx")}"

        }, 0x4E9595, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        // Manual Ramp

    }

    [UnmanagedCallersOnly]
    private static float ModifyTrickLandSpeed(float speed, Player* player)
    {
        OnSetPlayerSpeedOnTrickLand?.Invoke(speed, player);
        if (SetPlayerSpeedOnTrickLand != null)
            return SetPlayerSpeedOnTrickLand(speed, player);

        return speed;
    }
}