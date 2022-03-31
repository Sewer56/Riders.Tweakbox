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
    /// Checks if a function is to be executed after boosting.
    /// </summary>
    public static event PlayerAsmFunc CheckIfDisableAfterBoostFunction;

    private static IAsmHook _onCheckIfSetAfterBoostFunction;

    public void InitCheckIfSetAfterBoostFunction(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifDisable = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004CCCFE, false) };
        var asm = new[]
        {
            $"use32",
            $"mov [{(int)_tempPlayerPointer.Pointer}], edi",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfSetAfterBoostFunctionHook), ifDisable, null, null)}"
        };

        _onCheckIfSetAfterBoostFunction = hooks.CreateAsmHook(asm, 0x4CCCA2, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfSetAfterBoostFunctionHook()
    {
        return CheckIfDisableAfterBoostFunction?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
    }
}