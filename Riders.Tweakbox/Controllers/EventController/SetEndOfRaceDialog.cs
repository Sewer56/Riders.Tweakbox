using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Only applies to dialogs without the replay option!!
    /// We do a little hack in our Netplay code for the other ones.
    /// </summary>
    public static event SetEndOfRaceDialogHandlerFn SetEndOfRaceDialog;

    private static IHook<Functions.SetEndOfRaceDialogTaskFnPtr> _setEndOfRaceDialogTask;

    public void InitSetEndOfRaceDialog(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setEndOfRaceDialogTask = Functions.SetEndOfRaceDialogTask.HookAs<SetEndOfRaceDialogTaskFnPtr>(typeof(EventController), nameof(SetEndOfRaceDialogHandlerHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static Task* SetEndOfRaceDialogHandlerHook(EndOfRaceDialogMode mode)
    {
        return SetEndOfRaceDialog != null
            ? SetEndOfRaceDialog.Invoke(mode, _setEndOfRaceDialogTask)
            : _setEndOfRaceDialogTask.OriginalFunction.Value.Invoke(mode).Pointer;
    }

    public delegate Task* SetEndOfRaceDialogHandlerFn(EndOfRaceDialogMode mode, IHook<Functions.SetEndOfRaceDialogTaskFnPtr> hook);
}