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
    /// When the spawn location for all players is about to be set.
    /// </summary>
    public static event SetSpawnLocationsStartOfRaceFn OnSetSpawnLocationsStartOfRace;

    /// <summary>
    /// After the spawn location for all players has been set.
    /// </summary>
    public static event SetSpawnLocationsStartOfRaceFn AfterSetSpawnLocationsStartOfRace;

    private static IHook<StartLineSetSpawnLocationsFnPtr> _setSpawnLocationsStartOfRaceHook;

    public void InitSetSpawnLocationsStartOfRace()
    {
        _setSpawnLocationsStartOfRaceHook = Functions.SetSpawnLocationsStartOfRace.HookAs<StartLineSetSpawnLocationsFnPtr>(typeof(EventController), nameof(SetSpawnLocationsStartOfRaceHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static int SetSpawnLocationsStartOfRaceHook(int numberOfPlayers)
    {
        OnSetSpawnLocationsStartOfRace?.Invoke(numberOfPlayers);
        var result = _setSpawnLocationsStartOfRaceHook.OriginalFunction.Value.Invoke(numberOfPlayers);
        AfterSetSpawnLocationsStartOfRace?.Invoke(numberOfPlayers);
        return result;
    }

    public delegate void SetSpawnLocationsStartOfRaceFn(int numberOfPlayers);
}
