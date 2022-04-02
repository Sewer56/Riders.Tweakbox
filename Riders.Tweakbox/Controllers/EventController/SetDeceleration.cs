using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to override the amount of deceleration applied.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetDeceleration;

    private static IAsmHook _decelAsmHook;

    public void InitSetDeceleration(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _decelAsmHook = hooks.CreateAsmHook(new[]
        {
            "use32",

            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push ebx", // Player
            $"{utilities.AssembleAbsoluteCall<ModifyDecelerationFnPtr>(typeof(EventController), nameof(CalculateDecelerationHook), false)}",
            $"{utilities.PopFromX87ToXmm()}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x004BAAE2, AsmHookBehaviour.DoNotExecuteOriginal, 36).Activate();
    }

    [UnmanagedCallersOnly]
    private static float CalculateDecelerationHook(Player* player)
    {
        // Do decel based on Tweakbox settings.
        var newDecel = 0.0f;
        SetDeceleration?.Invoke(ref newDecel, player);
        return newDecel;
    }

    [Function(CallingConventions.Stdcall)]
    public struct ModifyDecelerationFnPtr { public FuncPtr<BlittablePointer<Player>, float> Value; }
}