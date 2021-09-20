using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Sets the speed acquired from dash panel.
    /// </summary>
    public static event SetItemboxRespawnTimerHandlerFn SetItemboxRespawnTimer;

    private static IAsmHook _setItemboxRespawnTimerHook;

    public void InitSetItemBoxRespawnTimer(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setItemboxRespawnTimerHook = hooks.CreateAsmHook(new string[]
        {
            // Goal: Modify `ecx` register
            "use32",
            $"{utilities.PushCdeclCallerSavedRegistersExcept("eax")}",
            $"{utilities.AssembleAbsoluteCall<SetItemboxRespawnTimerHandlerFnPtr>(typeof(EventController), nameof(SetItemboxRespawnTimerHook), false)}",
            $"{utilities.PopCdeclCallerSavedRegistersExcept("eax")}",
        }, 0x49B7BE, new AsmHookOptions()
        {
            Behaviour = AsmHookBehaviour.DoNotExecuteOriginal,
            PreferRelativeJump = true,
            hookLength = 17
        }).Activate();
    }

    [UnmanagedCallersOnly]
    private static int SetItemboxRespawnTimerHook()
    {
        if (SetItemboxRespawnTimer != null)
            return SetItemboxRespawnTimer();

        return 180;
    }
    
    public delegate int SetItemboxRespawnTimerHandlerFn();

    [Function(CallingConventions.Stdcall)]
    public struct SetItemboxRespawnTimerHandlerFnPtr { public FuncPtr<int> Value; }
}