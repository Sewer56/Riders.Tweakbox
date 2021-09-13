using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to override the running speed.
    /// </summary>
    public static event SetRunSpeedFn SetRunningSpeedHook;

    private static IHook<Functions.PlayerFnPtr> _setRunStateSpeedAndOtherStuff;

    public void InitSetRunStateSpeed(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setRunStateSpeedAndOtherStuff = Functions.HandleRunStateRunningSpeedAndSomeOtherStuff
            .HookAs<Functions.PlayerFnPtr>(typeof(EventController), nameof(SetNewRunningSpeedHook))
            .Activate();
    }

    [UnmanagedCallersOnly]
    private static int SetNewRunningSpeedHook(Player* player)
    {
        var originalRunSpeed = *Sewer56.SonicRiders.API.Player.RunPhysics2;
        var runSpeed = originalRunSpeed;

        SetRunningSpeedHook?.Invoke(player, &runSpeed);
        *Sewer56.SonicRiders.API.Player.RunPhysics2 = runSpeed;
        var result = _setRunStateSpeedAndOtherStuff.OriginalFunction.Value.Invoke(player);
        *Sewer56.SonicRiders.API.Player.RunPhysics2 = originalRunSpeed;

        return result;
    }
    
    public unsafe delegate void SetRunSpeedFn(Player* player, RunningPhysics2* physics);
}