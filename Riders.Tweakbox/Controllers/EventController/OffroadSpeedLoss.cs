using System;
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
    /// Use this event to override how the player loses speed while in off-road state.
    /// If this returns true, custom off road has been applied and vanilla should be skipped.
    /// </summary>
    public static event PerformOperationOnPlayerFn HandleCustomOffroadFn;

    private static IAsmHook _doOffroadSpeedLossHook;

    public void InitOffroadSpeedLoss(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _doOffroadSpeedLossHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push esi",
            $"{utilities.AssembleAbsoluteCall<PerformOperationOnPlayerFnPtr>(typeof(EventController), nameof(CustomOffroadDecelHook), false)}",
            $"cmp eax, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"jne exit",
            $"pop edi",
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4CB024, false)}",
            "exit:"
        }, 0x4CAFB5, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static byte CustomOffroadDecelHook(Player* player)
    {
        if (HandleCustomOffroadFn != null)
        {
            HandleCustomOffroadFn.Invoke(player);
            return 1;
        }

        return 0;
    }

}