using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Checks if a specific player is to be given a human indicator.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfPlayerIsHumanIndicator;

    private static IAsmHook _onCheckIfHumanInputIndicatorHook;

    public void InitCheckIfPlayerIsHumanIndicator(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifIsHumanIndicator = new string[] { "mov ecx, 0" };
        var ifIsNotHumanIndicator = new string[] { "mov ecx, 1" };
        var onCheckIsHumanIndicatorAsm = new[]
        {
            $"use32",
            $"{utilities.PushXmmRegisters(Constants.XmmRegisters)}",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfIsHumanIndicatorHook), ifIsHumanIndicator, ifIsNotHumanIndicator, null)}",
            $"{utilities.PopXmmRegisters(Constants.XmmRegisters)}",
        };

        _onCheckIfHumanInputIndicatorHook = hooks.CreateAsmHook(onCheckIsHumanIndicatorAsm, 0x004270D9, AsmHookBehaviour.ExecuteAfter).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfIsHumanIndicatorHook()
    {
        return OnCheckIfPlayerIsHumanIndicator?.Invoke(Sewer56.SonicRiders.API.Player.Players.Pointer) ??
               AsmFunctionResult.Indeterminate;
    }
}