using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Types;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Sets the speed acquired from dash panel.
    /// </summary>
    public static event SetDashPanelSpeedHandlerFn SetDashPanelSpeed;

    private static IAsmHook _setDashPanelSpeedHook;

    public void InitSetDashPanelSpeed(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setDashPanelSpeedHook = hooks.CreateAsmHook(new string[]
        {
            // Goal: Modify `ecx` register
            "use32",
            "push eax", // Caller Save Registers
            "push edx",
            $"{utilities.AssembleAbsoluteCall<SetDashPanelSpeedHandlerFnPtr>(typeof(EventController), nameof(SetDashPanelSpeedHook), false)}",
            "pop edx", // Caller Restore Registers
            "pop eax"
        }, 0x004C7AA5, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static IntFloat SetDashPanelSpeedHook(Player* player, float speed)
    {
        return SetDashPanelSpeed?.Invoke(player, speed) ?? new IntFloat(speed);
    }

    [Function(new[] { ebx, ecx }, ecx, StackCleanup.Caller)]
    public delegate IntFloat SetDashPanelSpeedHandlerFn(Player* player, float targetSpeed);

    [Function(new[] { ebx, ecx }, ecx, StackCleanup.Caller)]
    public struct SetDashPanelSpeedHandlerFnPtr { public FuncPtr<BlittablePointer<Player>, float, IntFloat> Value; }
}