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
    public static event GenericModifyPlayerIntFn OnSetPitAirGain;

    /// <summary>
    /// Sets the player's new drift cap.
    /// </summary>
    public static event GenericModifyPlayerIntFn SetPitAirGain;

    private static IAsmHook _setPitAirGain;

    public void InitSetPitAirGain(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setPitAirGain = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegistersExcept("eax")}",
            $"push edi",   // Push Player Ptr
            $"push eax",   // Value 
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerIntFnPtr>(typeof(EventController), nameof(SetPitAirGainHook), false)}",
            $"{utilities.PopCdeclCallerSavedRegistersExcept("eax")}",
        }, 0x4A174B, new AsmHookOptions() { MaxOpcodeSize = 5, PreferRelativeJump = true, Behaviour = AsmHookBehaviour.ExecuteAfter }).Activate();
    }

    [UnmanagedCallersOnly]
    private static int SetPitAirGainHook(int value, Player* player)
    {
        OnSetPitAirGain?.Invoke(value, player);
        if (SetPitAirGain != null)
            return SetPitAirGain(value, player);
        else
            return value;
    }
}