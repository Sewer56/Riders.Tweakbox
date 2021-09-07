using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Sets up the task that displays the new lap and results screen once the player crosses for a new lap.
    /// </summary>
    public static event SetGoalRaceFinishTaskHandlerFn SetGoalRaceFinishTask;

    private static IHook<Functions.SetGoalRaceFinishTaskFnPtr> _setGoalRaceFinishTaskHook;

    public void InitSetGoalRaceFinishTask(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setGoalRaceFinishTaskHook = Functions.SetGoalRaceFinishTask.HookAs<SetGoalRaceFinishTaskFnPtr>(typeof(EventController), nameof(SetGoalRaceFinishTaskHook)).Activate();
    }

    /// <summary>
    /// Invokes the original function for setting the `GOAL` splash on race finish.
    /// </summary>
    public void InvokeSetGoalRaceFinishTask(Sewer56.SonicRiders.Structures.Gameplay.Player* player) => _setGoalRaceFinishTaskHook.OriginalFunction.Value.Invoke(player);

    [UnmanagedCallersOnly]
    private static int SetGoalRaceFinishTaskHook(Sewer56.SonicRiders.Structures.Gameplay.Player* player)
    {
        return SetGoalRaceFinishTask?.Invoke(_setGoalRaceFinishTaskHook, player) ??
               _setGoalRaceFinishTaskHook.OriginalFunction.Value.Invoke(player);
    }

    public unsafe delegate int SetGoalRaceFinishTaskHandlerFn(IHook<Functions.SetGoalRaceFinishTaskFnPtr> hook, Sewer56.SonicRiders.Structures.Gameplay.Player* player);
}