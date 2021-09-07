using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Handler for the method which sets the task to render an item pickup.
    /// </summary>
    public static event SetRenderItemPickupTaskHandlerFn SetItemPickupTaskHandler;

    private static IHook<Functions.SetRenderItemPickupTaskFnPtr> _setRenderItemPickupTaskHook;

    public void InitSetItemPickupTaskHandler()
    {
        _setRenderItemPickupTaskHook = Functions.SetRenderItemPickupTask.HookAs<SetRenderItemPickupTaskFnPtr>(typeof(EventController), nameof(SetRenderItemPickupHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static Task* SetRenderItemPickupHook(Player* player, byte a2, ushort a3)
    {
        return SetItemPickupTaskHandler != null
            ? SetItemPickupTaskHandler(player, a2, a3, _setRenderItemPickupTaskHook)
            : _setRenderItemPickupTaskHook.OriginalFunction.Value.Invoke(player, a2, a3).Pointer;
    }

    public unsafe delegate Task* SetRenderItemPickupTaskHandlerFn(Player* player, byte a2, ushort a3, IHook<Functions.SetRenderItemPickupTaskFnPtr> hook);
}