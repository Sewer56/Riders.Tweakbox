using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed before the player's rings are to be drained after they die.
    /// </summary>
    public static event GenericPlayerFn OnSetRingsOnDeath;

    /// <summary>
    /// Use this event to override how the player's rings are set when the player is respawned after a death.
    /// </summary>
    public static event GenericPlayerFn SetRingsOnDeath;

    private static IAsmHook _setRingsOnDeathHook;

    public void InitSetRingsOnDeath(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setRingsOnDeathHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Original Code
            "mov [esi+0CB4h], ecx", // Original line to set ring count after is omitted.
            
            $"{utilities.PushXmmRegisters(Constants.XmmRegisters)}",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            "push esi", // Player Ptr
            $"{utilities.AssembleAbsoluteCall<GenericPlayerFnPtr>(typeof(EventController), nameof(OnPlayerDeathHook), false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"{utilities.PopXmmRegisters(Constants.XmmRegisters)}",

        }, 0x4B8EA6, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static void OnPlayerDeathHook(Player* player)
    {
        OnSetRingsOnDeath?.Invoke(player);
        if (SetRingsOnDeath != null)
            SetRingsOnDeath(player);
        else
            player->Rings = 0;
    }
}