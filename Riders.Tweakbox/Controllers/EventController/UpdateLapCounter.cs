using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Updates the player's lap counter.
    /// </summary>
    public static event UpdateLapCounterHandlerFn UpdateLapCounter;

    private static IHook<Functions.UpdateLapCounterFnPtr> _updateLapCounterHook;

    public void InitUpdateLapCounter(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _updateLapCounterHook = Functions.UpdateLapCounter.HookAs<UpdateLapCounterFnPtr>(typeof(EventController), nameof(UpdateLapCounterHook)).Activate();
    }

    /// <summary>
    /// Invokes the update lap counter original function.
    /// </summary>
    public void InvokeUpdateLapCounter(Player* player, int a2) => _updateLapCounterHook.OriginalFunction.Value.Invoke(player, a2);

    [UnmanagedCallersOnly]
    private static int UpdateLapCounterHook(Player* player, int a2)
    {
        return UpdateLapCounter?.Invoke(_updateLapCounterHook, player, a2) ??
               _updateLapCounterHook.OriginalFunction.Value.Invoke(player, a2);
    }

    public unsafe delegate int UpdateLapCounterHandlerFn(IHook<Functions.UpdateLapCounterFnPtr> hook, Player* player, int a2);
}