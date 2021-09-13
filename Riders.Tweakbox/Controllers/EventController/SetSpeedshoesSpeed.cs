using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Types;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.StackCleanup;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Sets the speed of the speed shoes item.
    /// </summary>
    public static event SetSpeedShoesSpeedHandlerFn SetSpeedShoesSpeed;

    private Pinnable<float> _speedshoeSpeed = new Pinnable<float>(0);
    private static IAsmHook _setSpeedshoesSpeedHook;

    public void InitSetSpeedshoesSpeed(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setSpeedshoesSpeedHook = hooks.CreateAsmHook(new string[]
        {
            // Goal: Modify `ecx` register
            "use32",
            "push eax", // Caller Save Registers
            "push edx",

            "mov ecx, dword [0x005BD4C8]",
            "push ecx",
            "push ebx",
            $"{utilities.AssembleAbsoluteCall<SetSpeedShoesSpeedHandlerFnPtr>(typeof(EventController), nameof(SetSpeedShoesSpeedHook), false)}",
            $"mov dword [{(int)_speedshoeSpeed.Pointer}], ecx",
            $"movss xmm0, [{(int)_speedshoeSpeed.Pointer}]",

            "pop edx", // Caller Restore Registers
            "pop eax"
        }, 0x004C7323, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static IntFloat SetSpeedShoesSpeedHook(Player* player, float speed)
    {
        SetSpeedShoesSpeed?.Invoke(player, ref speed);
        return new IntFloat(speed);
    }

    [Function(new FunctionAttribute.Register[0], ecx, Callee)]
    public delegate void SetSpeedShoesSpeedHandlerFn(Player* player, ref float targetSpeed);

    [Function(new FunctionAttribute.Register[0], ecx, Callee)]
    public struct SetSpeedShoesSpeedHandlerFnPtr { public FuncPtr<BlittablePointer<Player>, float, IntFloat> Value; }
}