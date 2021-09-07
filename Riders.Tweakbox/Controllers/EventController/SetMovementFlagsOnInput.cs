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
    /// Executed before the game gets the input flags based on player movements.
    /// </summary>
    public static event Functions.SetMovementFlagsBasedOnInputFn OnSetMovementFlagsOnInput;

    /// <summary>
    /// Executed after the game gets the input flags based on player movements.
    /// </summary>
    public static event Functions.SetMovementFlagsBasedOnInputFn AfterSetMovementFlagsOnInput;

    private static IHook<Functions.SetMovementFlagsBasedOnInputFnPtr> _setMovementFlagsOnInputHook;

    public void InitSetMovementFlagsOnInput()
    {
        _setMovementFlagsOnInputHook = Functions.SetMovementFlagsOnInput.HookAs<SetMovementFlagsBasedOnInputFnPtr>(typeof(EventController), nameof(OnSetMovementFlagsOnInputHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static Player* OnSetMovementFlagsOnInputHook(Player* player)
    {
        OnSetMovementFlagsOnInput?.Invoke(player);
        var result = _setMovementFlagsOnInputHook.OriginalFunction.Value.Invoke(player);
        AfterSetMovementFlagsOnInput?.Invoke(player);

        return result.Pointer;
    }
}