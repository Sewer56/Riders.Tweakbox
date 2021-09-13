using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to override the tornado deceleration.
    /// </summary>
    public static GenericModifyPlayerFloatFn SetTornadoDeceleration;

    private static IAsmHook _setTornadoDeceleration;

    public void InitSetTornadoDeceleration(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setTornadoDeceleration = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.PushXmmRegister("xmm0")}",

            "push ecx",               // Player Ptr
            "push dword [ecx+1134h]", // Tornado Decel Speed
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetTornadoDecelerationHook), false)}",

            // Restore XMM and multiply with new value
            $"{utilities.PopXmmRegister("xmm0")}",
            $"sub esp, 4",
            $"fstp dword [esp]",
            $"mulss xmm0, dword [esp]", // Original instruction replacement
            $"add esp, 4",
            $"{utilities.PopCdeclCallerSavedRegisters()}"

        }, 0x00451817, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static float SetTornadoDecelerationHook(float value, Player* player)
    {
        if (SetTornadoDeceleration != null)
            return SetTornadoDeceleration(value, player);

        return value;
    }
}