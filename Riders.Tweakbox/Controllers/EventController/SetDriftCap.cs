using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed before the drift cap for a specific player is set.
    /// </summary>
    public static event GenericModifyPlayerFloatFn OnSetNewDriftCap;

    /// <summary>
    /// Sets the player's new drift cap.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetNewDriftCap;

    private static IAsmHook _setDriftCapHook;

    public void InitSetDriftCap(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setDriftCapHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",

            $"push ebx",                // Push Player Ptr
            $"push dword [0x5BCCA0]",   // Float Value 
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetNewDriftCapHook), false)}",
            $"{utilities.PopFromX87ToXmm("xmm1")}", // Replaces original code line.

            $"{utilities.PopCdeclCallerSavedRegisters()}",
        }, 0x4E501D, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static float SetNewDriftCapHook(float value, Player* player)
    {
        OnSetNewDriftCap?.Invoke(value, player);
        if (SetNewDriftCap != null)
            return SetNewDriftCap(value, player);
        else
            return value;
    }
}