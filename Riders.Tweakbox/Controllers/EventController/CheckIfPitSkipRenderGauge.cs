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
    public static event PlayerAsmFunc CheckIfPitSkipRenderGauge;

    private static IAsmHook _onCheckIfSkipRenderGaugeFill;

    public void InitCheckIfPitSkipRenderGauge(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifSkipRenderGauge = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004A17C0, Environment.Is64BitProcess) };
        var onCheckIfSkipRenderGaugeAsm = new[]
        {
            $"use32",
            $"mov [{(int)_tempPlayerPointer.Pointer}], edi", 
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfSkipRenderGaugeHook), ifSkipRenderGauge, null, null)}"
        };
        _onCheckIfSkipRenderGaugeFill = hooks.CreateAsmHook(onCheckIfSkipRenderGaugeAsm, 0x004A178C, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfSkipRenderGaugeHook()
    {
        return CheckIfPitSkipRenderGauge?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
    }
}