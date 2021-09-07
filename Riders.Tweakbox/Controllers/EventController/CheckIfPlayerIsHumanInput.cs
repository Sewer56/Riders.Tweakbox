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
    /// Checks if a specific player is a human character.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfPlayerIsHumanInput;

    private static IAsmHook _onCheckIsHumanInputHook;

    public void InitCheckIfPlayerIsHumanInput(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var ifIsHumanInput = new string[] { "mov edx, 0" };
        var ifIsNotHumanInput = new string[] { "mov edx, 1" };
        var onCheckIsHumanInputAsm = new[]
        {
            $"use32",
            $"mov [{(int)_tempPlayerPointer.Pointer}], esi", 
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfIsHumanInputHook), ifIsHumanInput, ifIsNotHumanInput, null)}"
        };

        _onCheckIsHumanInputHook = hooks.CreateAsmHook(onCheckIsHumanInputAsm, 0x004BD0C4, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfIsHumanInputHook()
    {
        return OnCheckIfPlayerIsHumanInput?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
    }
}