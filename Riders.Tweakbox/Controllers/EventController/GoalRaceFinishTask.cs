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
    /// The task used to render the race finish sequence after the final player crosses the finish line.
    /// </summary>
    public static event ReturnByteFnHandler GoalRaceFinishTask;

    private static IHook<Functions.CdeclReturnByteFnPtr> _goalRaceFinishTaskHook;

    public void InitGoalRaceFinishTask(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _goalRaceFinishTaskHook = Functions.GoalRaceFinishTask.HookAs<CdeclReturnByteFnPtr>(typeof(EventController), nameof(GoalRaceFinishTaskHook)).Activate();
    }
    
    [UnmanagedCallersOnly]
    private static byte GoalRaceFinishTaskHook()
    {
        return GoalRaceFinishTask?.Invoke(_goalRaceFinishTaskHook) ??
               _goalRaceFinishTaskHook.OriginalFunction.Value.Invoke();
    }
}