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
    /// Performed when the check for wall bump speed loss is performed.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfSpeedLossFromWallHit;

    /// <summary>
    /// Returning false cancels speed loss.
    /// Returning true forces speed loss.
    /// Returning indeterminate doesn't do anything. 
    /// </summary>
    public static event PlayerAsmFunc SetSpeedLossFromWallHit;

    private static IAsmHook _onWallBumpHook;

    public void InitSpeedLossFromWallHit(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        // Wall Bump Speed Override
        var overrideWallBumpRegisters = new string[] { "xmm0", "xmm1" };
        var ifForceWallBumpAsm = new string[]
        {
            $"{utilities.PopXmmRegisters(overrideWallBumpRegisters)}",
            utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4C1A4A, false)
        };
        var ifForceNotWallBumpAsm = new string[]
        {
            $"{utilities.PopXmmRegisters(overrideWallBumpRegisters)}",
            utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4C1AA2, false)
        };
        var onOverrideWallBumpBehaviourAsm = new[]
        {
            $"use32",
            $"{utilities.PushXmmRegisters(overrideWallBumpRegisters)}",
            $"mov dword [{(int)_tempPlayerPointer.Pointer}], esi",
            utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnOverrideWallBumpBehaviour), ifForceWallBumpAsm, ifForceNotWallBumpAsm, null),
            $"{utilities.PopXmmRegisters(overrideWallBumpRegisters)}",
        };
        _onWallBumpHook = hooks.CreateAsmHook(onOverrideWallBumpBehaviourAsm, 0x4C1A3F, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnOverrideWallBumpBehaviour()
    {
        OnCheckIfSpeedLossFromWallHit?.Invoke(_tempPlayerPointer.Pointer->Pointer);
        if (SetSpeedLossFromWallHit != null)
            return SetSpeedLossFromWallHit(_tempPlayerPointer.Pointer->Pointer);

        return (int)AsmFunctionResult.Indeterminate;
    }
}