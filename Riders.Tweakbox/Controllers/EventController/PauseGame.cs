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
    /// Replaces the code to pause the game.
    /// </summary>
    public static event PauseGameHandlerFn PauseGame;

    private static IHook<Functions.PauseGameFnPtr> _pauseGameHook;

    public void InitPauseGame(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _pauseGameHook = Functions.PauseGame.HookAs<PauseGameFnPtr>(typeof(EventController), nameof(PauseGameHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static int PauseGameHook(int a1, int a2, byte a3) => PauseGame?.Invoke(a1, a2, a3, _pauseGameHook) ?? _pauseGameHook.OriginalFunction.Value.Invoke(a1, a2, a3);

    public delegate int PauseGameHandlerFn(int a1, int a2, byte a3, IHook<Functions.PauseGameFnPtr> hook);
}