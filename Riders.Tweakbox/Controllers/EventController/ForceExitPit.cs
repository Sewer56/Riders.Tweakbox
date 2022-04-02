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
    /// Performed when the check for pit exit is performed.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfExitPit;

    /// <summary>
    /// Returning false cancels pit exit.
    /// Returning true forces pit exit.
    /// Returning indeterminate doesn't do anything. 
    /// </summary>
    public static event PlayerAsmFunc SetForceExitPit;

    /// <summary>
    /// Executed when the player exits a pit.
    /// </summary>
    public static event GenericPlayerFn OnExitPit;

    private static IAsmHook _forceExitPitHook;
    private static IAsmHook _onExitPitHook;

    public void InitForceExitPit(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifForceExitPit = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4A1902, false) };
        var ifForceNotExitPit = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4A16B5, false) };
        var onCheckIfOverrideExitPit = new[]
        {
            $"use32",
            $"mov dword [{(int)_tempPlayerPointer.Pointer}], edi",
            utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(CheckIfForceExitPitHook), ifForceExitPit, ifForceNotExitPit, null)
        };

        _forceLegendEffectStateHook = hooks.CreateAsmHook(onCheckIfOverrideExitPit, 0x4A18D0, AsmHookBehaviour.ExecuteFirst).Activate();

        var onExitPit = new[]
        {
            $"use32",
            $"mov dword [{(int)_tempPlayerPointer.Pointer}], edi",
            utilities.AssembleAbsoluteCall<AsmActionPtr>(typeof(EventController), nameof(OnExitPitHook))
        };

        _onExitPitHook = hooks.CreateAsmHook(onExitPit, 0x4A1902, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int CheckIfForceExitPitHook()
    {
        OnCheckIfExitPit?.Invoke(_tempPlayerPointer.Pointer->Pointer);
        if (SetForceExitPit != null)
            return SetForceExitPit(_tempPlayerPointer.Pointer->Pointer);

        return (int)AsmFunctionResult.Indeterminate;
    }

    [UnmanagedCallersOnly]
    private static void OnExitPitHook() => OnExitPit?.Invoke(_tempPlayerPointer.Pointer->Pointer);
}