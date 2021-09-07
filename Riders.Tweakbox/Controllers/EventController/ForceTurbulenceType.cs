using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Utility;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.StackCleanup;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Allows you to force a specific turbulence type.
    /// </summary>
    public static event ForceTurbulenceTypeFn ForceTurbulenceType;

    private static IAsmHook _forceTurbulenceTypeHook;

    public void InitForceTurbulenceType(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {

        _forceTurbulenceTypeHook = hooks.CreateAsmHook(new string[]
        {
            // Goal: Modify `ax` register
            "use32",
            "push ecx", // Caller Save Registers
            "push edx",
            $"{utilities.AssembleAbsoluteCall<ForceTurbulenceTypeFnPtr>(typeof(EventController), nameof(ForceTurbulenceTypeHook), false)}",
            "pop edx", // Caller Restore Registers
            "pop ecx"
        }, 0x0045617F, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    [UnmanagedCallersOnly]
    private static int ForceTurbulenceTypeHook(byte type) => ForceTurbulenceType?.Invoke(type) ?? type;

    [Function(eax, eax, Caller)]
    public delegate int ForceTurbulenceTypeFn(byte currentType);

    [Function(eax, eax, Caller)]
    public struct ForceTurbulenceTypeFnPtr { public FuncPtr<byte, int> Value; }
}