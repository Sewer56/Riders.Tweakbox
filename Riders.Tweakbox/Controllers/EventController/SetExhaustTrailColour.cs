using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Misc;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to set the exhaust trail colour for a gear.
    /// </summary>
    public static GenericModifyPlayerColourFn SetExhaustTrailColour;

    private static IHook<Functions.CdeclReturnByteFnPtr> _exhaustTrailTaskHook;

    public void InitSetExhaustTrail(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _exhaustTrailTaskHook = Functions.ExhaustTrailTask.HookAs<Functions.CdeclReturnByteFnPtr>(typeof(EventController), nameof(ExhaustTrailTaskHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static byte ExhaustTrailTaskHook()
    {
        var task   = *(Task<ExhaustTrail>**)State.CurrentTask;
        var taskData = task->TaskData;
        var player = taskData->Player;

        if (player == null)
            return _exhaustTrailTaskHook.OriginalFunction.Value.Invoke();

        if (task->TaskStatus == 3 && SetExhaustTrailColour != null)
        {
            var originalColour = taskData->ExhaustTrailColour;
            SetExhaustTrailColour(&taskData->ExhaustTrailColour, player);
            var result = _exhaustTrailTaskHook.OriginalFunction.Value.Invoke();
            taskData->ExhaustTrailColour = originalColour;
            return result;
        }

        return _exhaustTrailTaskHook.OriginalFunction.Value.Invoke();
    }

    /// <summary>
    /// Generic function that modifies a float in memory for a given player.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate void GenericModifyPlayerColourFn(ColorRGBA* value, Player* player);
}