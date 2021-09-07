using System.Runtime.InteropServices;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed before the player's air gained for this frame is set for the final time.
    /// </summary>
    public static event GenericPlayerFn OnSetAirGainedThisFrame;

    /// <summary>
    /// Use this event to override how the player's air gained for this frame is set.
    /// </summary>
    public static event GenericPlayerFn SetAirGainedThisFrame;

    private static IAsmHook _setAirGainedLastFrameHook;

    public void InitSetAirGainedLastFrame(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setAirGainedLastFrameHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"push eax\npush edx", // Caller Save Registers
            "push esi", // Player Ptr
            $"{utilities.AssembleAbsoluteCall<GenericPlayerFnPtr>(typeof(EventController), nameof(OnSetAirGainedThisFrameHook), false)}",
            $"mov ecx, dword [esi+0ABCh]", // Copy air gained this frame to register storing copy.
            $"pop edx\npop eax", // Caller Restore Registers

            // Original Code
            "test eax, eax",
        }, 0x4BD11B, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnSetAirGainedThisFrameHook(Player* player)
    {
        OnSetAirGainedThisFrame?.Invoke(player);
        if (SetAirGainedThisFrame != null)
            SetAirGainedThisFrame(player);
        else // Original code we overwrote.
            player->AirGainedThisFrame *= (player->GearSpecialFlags.HasAllFlags(ExtremeGearSpecialFlags.GearOnRings) ? 0 : 1);
    }
}