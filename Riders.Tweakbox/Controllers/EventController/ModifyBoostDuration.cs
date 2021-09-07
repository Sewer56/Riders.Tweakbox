using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executes as the player's boost duration is set.
    /// </summary>
    public static GenericModifyPlayerIntFn OnSetBoostDuration;

    /// <summary>
    /// Modifies the player's boost duration.
    /// </summary>
    public static GenericModifyPlayerIntFn SetBoostDuration;

    private IAsmHook _setBoostDurationHook;

    public void InitModifyBoostDuration(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setBoostDurationHook = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegistersExcept("eax")}",
            "push esi", // Player Ptr
            "push eax", // Boost Duration
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerIntFnPtr>(typeof(EventController), nameof(ModifyBoostDurationHook), false)}",
            $"{utilities.PopCdeclCallerSavedRegistersExcept("eax")}",

        }, 0x4CDBD2, new AsmHookOptions()
        {
            PreferRelativeJump = true,
            MaxOpcodeSize = 5,
            Behaviour = AsmHookBehaviour.ExecuteAfter
        }).Activate();
    }

    [UnmanagedCallersOnly]
    private static int ModifyBoostDurationHook(int duration, Player* player)
    {
        OnSetBoostDuration?.Invoke(duration, player);
        if (SetBoostDuration == null)
            return duration;

        return SetBoostDuration(duration, player);
    }
}