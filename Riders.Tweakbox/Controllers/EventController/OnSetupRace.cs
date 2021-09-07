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

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Provides a "last-chance" event to modify stage load properties, such as the number of players
    /// or cameras to be displayed after stage load. Consider some fields in the <see cref="State"/> class.
    /// </summary>
    public static event SetupRaceFn OnSetupRace;

    private static IAsmHook _onSetupRaceSettingsHook;

    public void InitOnSetupRace(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _onSetupRaceSettingsHook = hooks.CreateAsmHook(new[]
        {
            $"use32",
            $"{utilities.AssembleAbsoluteCall(typeof(EventController), nameof(OnSetupRaceHook))}"
        }, 0x0046C139, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static void OnSetupRaceHook() => OnSetupRace?.Invoke((Task<TitleSequence, TitleSequenceTaskState>*)(*State.CurrentTask));

    public delegate void SetupRaceFn(Task<TitleSequence, TitleSequenceTaskState>* task);
}