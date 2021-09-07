using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Handles the player state event.
    /// The handler assigned to this event is responsible for calling the original function.
    /// </summary>
    public static event SetNewPlayerStateHandlerFn SetNewPlayerStateHandler;

    private static IHook<Functions.SetNewPlayerStateFnPtr> _setNewPlayerStateHook;

    public void InitSetNewPlayerStateHandler()
    {
        _setNewPlayerStateHook = Functions.SetPlayerState.HookAs<SetNewPlayerStateFnPtr>(typeof(EventController), nameof(SetPlayerStateHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static byte SetPlayerStateHook(Player* player, PlayerState state)
    {
        return SetNewPlayerStateHandler?.Invoke(player, state, _setNewPlayerStateHook) ?? _setNewPlayerStateHook.OriginalFunction.Value.Invoke(player, state);
    }

    public unsafe delegate byte SetNewPlayerStateHandlerFn(Player* player, PlayerState state, IHook<Functions.SetNewPlayerStateFnPtr> hook);
}