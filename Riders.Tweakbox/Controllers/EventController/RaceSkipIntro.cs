using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed when the stage intro is skipped.
    /// </summary>
    public static event AsmAction OnRaceSkipIntro;

    /// <summary>
    /// Queries the user whether the intro should be skipped.
    /// </summary>
    public static event AsmFunc OnCheckIfSkipIntro;

    private static IAsmHook _skipIntroCameraHook;
    private static IAsmHook _checkIfSkipIntroCamera;

    public void InitRaceSkipIntro(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var onSkipIntroAsm = new[]
        {
            $"use32", 
            $"{utilities.AssembleAbsoluteCall(typeof(EventController), nameof(OnRaceSkipIntroHook))}"
        };

        var ifSkipIntroAsm = new string[]
        {
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, Environment.Is64BitProcess)}"
        };

        var onCheckIfSkipIntroAsm = new[]
        {
            $"use32",
            $"{utilities.AssembleAbsoluteCall<AsmFuncPtr>(typeof(EventController), nameof(OnCheckIfSkipIntroHook), ifSkipIntroAsm, null, null)}"
        };

        _skipIntroCameraHook = hooks.CreateAsmHook(onSkipIntroAsm, 0x00416001, AsmHookBehaviour.ExecuteFirst).Activate();
        _checkIfSkipIntroCamera = hooks.CreateAsmHook(onCheckIfSkipIntroAsm, 0x415F2F, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnCheckIfSkipIntroHook() => OnCheckIfSkipIntro.InvokeIfNotNull();

    [UnmanagedCallersOnly]
    private static void OnRaceSkipIntroHook() => OnRaceSkipIntro?.Invoke();
}