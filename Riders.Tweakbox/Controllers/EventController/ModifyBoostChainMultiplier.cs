using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed as the player is about to boost chain.
    /// </summary>
    public static event GenericModifyPlayerFloatFn OnBoostChain;

    /// <summary>
    /// Allows to override a player's boost chain multiplier.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetBoostChainMultiplier;

    private IAsmHook _setBoostChainMultiplier;

    public void InitBoostChainMultiplier(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setBoostChainMultiplier = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.PushXmmRegisterFloat("xmm0")}",
            "push esi",              // Player Ptr
            "push dword [0x5C31A0]", // Boost Chain Multiplier
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetNewBoostChainMultiplier), false)}",
            $"{utilities.PopXmmRegisterFloat("xmm0")}",
            
            // Pop from X87 Stack and perform multiply from original code.
            $"sub esp, 4", 
            $"fstp dword [esp]",
            "mulss xmm0, [esp]", // Original Code
            $"add esp, 4",
            
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x4CE227, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static float SetNewBoostChainMultiplier(float value, Player* player)
    {
        OnBoostChain?.Invoke(value, player);
        if (SetBoostChainMultiplier == null)
            return value;

        return SetBoostChainMultiplier(value, player);
    }
}