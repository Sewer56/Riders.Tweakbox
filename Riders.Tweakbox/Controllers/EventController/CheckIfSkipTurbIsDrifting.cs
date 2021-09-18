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
    /// Checks if the rendering of the filling up of gauge (when pitted) should be rendered.
    /// </summary>
    public static event PlayerAsmFunc CheckIfSkipTurbChecks;

    private static IAsmHook _onCheckIfSkipTurbChecksHook;

    public void InitCheckIfTurbIsDrifting(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var onCheckIfSkipTurbChecksAsm = new[]
        {
            $"use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"mov [{(int)_tempPlayerPointer.Pointer}], ebx",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfSkipTurbChecksHook), false)}",
            $"cmp eax, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"jne exit",
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x457EED, false)}",
            $"exit:"
        };

        _onCheckIfSkipTurbChecksHook = hooks.CreateAsmHook(onCheckIfSkipTurbChecksAsm, 0x00457E92, new AsmHookOptions()
        {
            Behaviour = AsmHookBehaviour.ExecuteFirst,
            PreferRelativeJump = true,
            MaxOpcodeSize = 5
        }).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfSkipTurbChecksHook()
    {
        return CheckIfSkipTurbChecks?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
    }
}