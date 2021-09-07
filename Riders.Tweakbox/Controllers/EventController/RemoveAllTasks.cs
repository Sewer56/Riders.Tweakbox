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
    /// Executed when all tasks are about to be removed from the heap.
    /// </summary>
    public static event ReturnIntFnHandler RemoveAllTasks;

    private static IHook<Functions.CdeclReturnIntFnPtr> _removeAllTasksHook;

    public void InitRemoveAllTasks(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _removeAllTasksHook = Functions.RemoveAllTasks.HookAs<CdeclReturnIntFnPtr>(typeof(EventController), nameof(RemoveAllTasksHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static int RemoveAllTasksHook()
    {
        return RemoveAllTasks?.Invoke(_removeAllTasksHook) ?? _removeAllTasksHook.OriginalFunction.Value.Invoke();
    }
}