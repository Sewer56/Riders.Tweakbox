using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Utility;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.StackCleanup;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to force a specific turbulence type.
    /// </summary>
    public static event PlayerAsmFunc CheckIfIgnoreTurbulence;

    private static IAsmHook _ignoreTurbulenceCollisionHook;

    public void InitIgnoreTurbulenceCollision(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var asm = new string[]
        {
            $"use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push ebx", // player
            $"{utilities.AssembleAbsoluteCall<PlayerAsmFuncPtr>(typeof(EventController), nameof(CheckifIgnoreTurbulenceHook), false)}",
            $"cmp eax, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"jne exit",
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00459A5E, false)}",
            "exit:"
        };

        _ignoreTurbulenceCollisionHook = hooks.CreateAsmHook(asm, 0x00459760, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int CheckifIgnoreTurbulenceHook(Player* player)
    {
        if (CheckIfIgnoreTurbulence != null)
            return CheckIfIgnoreTurbulence(player);

        return (int)AsmFunctionResult.Indeterminate;
    }
}