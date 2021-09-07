using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed when an attack is executed by the game.
    /// </summary>
    public static event Functions.StartAttackTaskFn OnStartAttackTask;

    /// <summary>
    /// Executed when an attack is executed by the game before <see cref="OnStartAttackTask"/>.
    /// If this returns 1, execution of original code will be omitted.
    /// </summary>
    public static event Functions.StartAttackTaskFn OnShouldRejectAttackTask;

    /// <summary>
    /// Executed after an attack is executed by the game.
    /// </summary>
    public static event Functions.StartAttackTaskFn AfterStartAttackTask;

    private static IHook<Functions.StartAttackTaskFnPtr> _startAttackTaskHook;

    public void InitStartAttackTask()
    {
        _startAttackTaskHook = Functions.StartAttackTask.HookAs<StartAttackTaskFnPtr>(typeof(EventController), nameof(OnStartAttackTaskHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static int OnStartAttackTaskHook(Player* playerOne, Player* playerTwo, int a3)
    {
        var reject = OnShouldRejectAttackTask?.Invoke(playerOne, playerTwo, a3);
        if (reject.HasValue && reject.Value == 1)
            return 0;

        OnStartAttackTask?.Invoke(playerOne, playerTwo, a3);
        var result = _startAttackTaskHook.OriginalFunction.Value.Invoke(playerOne, playerTwo, a3);
        AfterStartAttackTask?.Invoke(playerOne, playerTwo, a3);
        return result;
    }
}