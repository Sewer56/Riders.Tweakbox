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
    /// Checks if a specific player is to be forced to level up.
    /// </summary>
    public static event PlayerAsmFunc ForceLevelUpHandler;

    /// <summary>
    /// Checks if a specific player is to be forced to level down.
    /// </summary>
    public static event PlayerAsmFunc ForceLevelDownHandler;

    private IAsmHook _initForceLevelUpHook;
    private IAsmHook _initForceLevelDownHook;

    public void InitForceLevelUpInRace(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        string[] ifLevelUp = new string[]
        {
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x0042DC1E, false)}",
        };

        string[] ifNotLevelUp = new string[]
        {
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x0042DD4B, false)}"
        };
        
        _initForceLevelUpHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"mov [{(int)_tempPlayerPointer.Pointer}], edi",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(SetForceLevelUpInRace), false)}",
            $"cmp eax, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"{utilities.AssembleTrueFalseForAsmFunctionResult(ifLevelUp, ifNotLevelUp, null)}"

        }, 0x42DBF6, new AsmHookOptions()
        {
            Behaviour = AsmHookBehaviour.ExecuteAfter,
            MaxOpcodeSize = 5,
            PreferRelativeJump = true
        }).Activate();

        string[] ifLevelDown = new string[]
        {
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x0042DD5D, false)}",
        };

        string[] ifNotLevelDown = new string[]
        {
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x0042DDA6, false)}"
        };
        
        _initForceLevelDownHook = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"mov [{(int)_tempPlayerPointer.Pointer}], edi",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(SetForceLevelDownInRace), false)}",
            $"cmp eax, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"{utilities.AssembleTrueFalseForAsmFunctionResult(ifLevelDown, ifNotLevelDown, null)}"

        }, 0x42DD4B, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    public static int SetForceLevelUpInRace()
    {
        if (ForceLevelUpHandler == null)
            return (int) AsmFunctionResult.Indeterminate;
        
        return (int) ForceLevelUpHandler(_tempPlayerPointer.Pointer->Pointer);
    }

    [UnmanagedCallersOnly]
    public static int SetForceLevelDownInRace()
    {
        if (ForceLevelDownHandler == null)
            return (int)AsmFunctionResult.Indeterminate;

        return (int)ForceLevelDownHandler(_tempPlayerPointer.Pointer->Pointer);
    }

}