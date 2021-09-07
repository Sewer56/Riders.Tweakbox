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
    /// Allows you to determine whether the game should spawn turbulence based off of player parameters.
    /// </summary>
    public static event ShouldSpawnTurbulenceHandlerFn ShouldSpawnTurbulence;

    private static IHook<Functions.ShouldGenerateTurbulenceFnPtr> _shouldGenerateTurbulenceHook;

    public void InitShouldSpawnTurbulence(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _shouldGenerateTurbulenceHook = Functions.ShouldSpawnTurbulence.HookAs<ShouldGenerateTurbulenceFnPtr>(typeof(EventController), nameof(ShouldGenerateTurbulenceHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static byte ShouldGenerateTurbulenceHook(Player* player)
    {
        if (ShouldSpawnTurbulence != null)
            return (byte)(ShouldSpawnTurbulence(player, _shouldGenerateTurbulenceHook) ? 1 : 0);
        
        return _shouldGenerateTurbulenceHook.OriginalFunction.Value.Invoke(player);
    }

    public unsafe delegate bool ShouldSpawnTurbulenceHandlerFn(Player* player, IHook<Functions.ShouldGenerateTurbulenceFnPtr> hook);
}