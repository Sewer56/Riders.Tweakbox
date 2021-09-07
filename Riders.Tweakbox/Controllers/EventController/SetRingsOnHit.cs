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
    /// Executed before the player's rings are to be drained after they are hit.
    /// Applies only to modes where rings are lost (i.e. not triggered by battle).
    /// </summary>
    public static event GenericPlayerFn OnSetRingsOnHit;

    /// <summary>
    /// Use this event to override how the player's rings are set when the player is hit.
    /// </summary>
    public static event GenericPlayerFn SetRingsOnHit;

    private static IAsmHook _setRingsOnHitHook;

    public void InitSetRingsOnHit(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setRingsOnHitHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            $"{utilities.PushCdeclCallerSavedRegisters()}",
            "push esi", // Player Ptr
            $"{utilities.AssembleAbsoluteCall<GenericPlayerFnPtr>(typeof(EventController), nameof(OnPlayerHitRingLossHook), false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

            // Original Code
            "cmp ebx, 8", // Original line to set ring count after is omitted.
        }, 0x4A456D, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static void OnPlayerHitRingLossHook(Player* player)
    {
        OnSetRingsOnHit?.Invoke(player);
        if (SetRingsOnHit != null)
            SetRingsOnHit.Invoke(player);
        else
            player->Rings = 0;
    }
}