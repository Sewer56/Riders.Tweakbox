using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed when the Enter key is pressed to start a race in character select.
    /// </summary>
    public static event AsmAction OnStartRace;

    /// <summary>
    /// Queries the user whether the race should be started.
    /// </summary>
    public static event AsmFunc OnCheckIfStartRace;

    private static IAsmHook _onStartRaceHook;
    private static IAsmHook _onCheckIfStartRaceHook;

    public void InitOnStartRace(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        var onStartRaceAsm = new[] 
        {
            $"use32",
            $"{utilities.AssembleAbsoluteCall(typeof(EventController), nameof(OnStartRaceHook), true)}"
        };

        var ifStartRaceAsm = new string[]
        {
            utilities.GetAbsoluteJumpMnemonics((IntPtr)0x0046364B, Environment.Is64BitProcess)
        };

        var onCheckIfStartRaceAsm = new[]
        {
            $"use32",
            $"{utilities.AssembleAbsoluteCall(() => OnCheckIfStartRace.InvokeIfNotNull(), ifStartRaceAsm, null, null)}"
        };

        _onStartRaceHook = hooks.CreateAsmHook(onStartRaceAsm, 0x0046364B, AsmHookBehaviour.ExecuteFirst).Activate();
        _onCheckIfStartRaceHook = hooks.CreateAsmHook(onCheckIfStartRaceAsm, 0x0046352B, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static void OnStartRaceHook() => OnStartRace?.Invoke();
}