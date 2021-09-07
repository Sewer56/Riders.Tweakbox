using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Performed when the check for legend effect is performed.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfLegendEffect;

    /// <summary>
    /// Returning false cancels legend effect.
    /// Returning true forces legend effect.
    /// Returning indeterminate doesn't do anything. 
    /// </summary>
    public static event PlayerAsmFunc SetForceLegendEffect;

    private static IAsmHook _forceLegendEffectStateHook;

    public void InitForceLegendEffect(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifForceLegendAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4BDC1A, false) };
        var ifForceNotLegendAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4BDC45, false) };
        var onCheckIfOverrideLegendAsm = new[]
        {
            $"use32",
            $"mov dword [{(int)_tempPlayerPointer.Pointer}], esi",
            utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(CheckIfOverrideLegendHook), ifForceLegendAsm, ifForceNotLegendAsm, null)
        };
        _forceLegendEffectStateHook = hooks.CreateAsmHook(onCheckIfOverrideLegendAsm, 0x4BDC11, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int CheckIfOverrideLegendHook()
    {
        OnCheckIfLegendEffect?.Invoke(_tempPlayerPointer.Pointer->Pointer);
        if (SetForceLegendEffect != null)
            return SetForceLegendEffect(_tempPlayerPointer.Pointer->Pointer);

        return (int)AsmFunctionResult.Indeterminate;
    }
}