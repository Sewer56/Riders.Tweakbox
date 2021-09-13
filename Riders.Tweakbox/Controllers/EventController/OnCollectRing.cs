using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed as the ring count from ring pickup is set.
    /// </summary>
    public static event GenericModifyPlayerIntFn OnSetRingCountFromRingPickup;

    /// <summary>
    /// Sets the new ring count after picking up a ring.
    /// </summary>
    public static event GenericModifyPlayerIntFn SetRingCountFromRingPickup;

    private static IAsmHook _setRingPickupHook;

    public void InitOnCollectRing(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setRingPickupHook = hooks.CreateAsmHook(new[]
        {
            $"use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            "push ebx",
            "push edi",
            $"{utilities.AssembleAbsoluteCall(typeof(EventController), nameof(PickupRingHook), false)}",
            $"mov edi, eax",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x004C7C74, new AsmHookOptions() { Behaviour = AsmHookBehaviour.ExecuteFirst, MaxOpcodeSize = 5, PreferRelativeJump = true }).Activate();
    }

    [UnmanagedCallersOnly]
    private static int PickupRingHook(int value, Player* player)
    {
        var copy = value;
        OnSetRingCountFromRingPickup?.Invoke(ref copy, player);
        SetRingCountFromRingPickup?.Invoke(ref value, player);

        return value;
    }
}