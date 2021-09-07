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
    /// Allows you to determine whether the game should NOT spawn turbulence based off of player parameters.
    /// </summary>
    public static event ShouldKillTurbulenceHandlerFn ShouldKillTurbulence;

    private static IHook<Functions.ShouldKillTurbulenceFnPtr> _shouldKillTurbulenceHook;

    public void InitShouldKillTurbulence(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _shouldKillTurbulenceHook = Functions.ShouldNotGenerateTurbulence.HookAs<ShouldKillTurbulenceFnPtr>(typeof(EventController), nameof(ShouldKillTurbulenceHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static byte ShouldKillTurbulenceHook(Player* player)
    {
        if (ShouldKillTurbulence != null)
            return (byte)(ShouldKillTurbulence(player, _shouldKillTurbulenceHook) ? 1 : 0);

        return _shouldKillTurbulenceHook.OriginalFunction.Value.Invoke(player);
    }

    public unsafe delegate bool ShouldKillTurbulenceHandlerFn(Player* player, IHook<Functions.ShouldKillTurbulenceFnPtr> hook);
}