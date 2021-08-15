using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Shared;
using Riders.Tweakbox.Shared.Structs;
namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Implementation of <see cref="DecelProperties"/> in x86 SSE assembly.
/// </summary>
public class DecelerationController : IController
{
    private IAsmHook _decelAsmHook;

    public unsafe DecelerationController(IReloadedHooks hooks)
    {
        var dummy = DecelMode.Default;
        var ptr = Static.DecelProperties.Pointer;

        _decelAsmHook = hooks.CreateAsmHook(new[]
        {
            "use32",

            // Get mode
            $"mov cl, byte [{(int)(&ptr->Mode)}]",
            $"cmp cl, {(int)DecelMode.Linear}",
            "je linear",

            "original:",
            "movss xmm1, [0x5C08FC]",
            "subss xmm1, [ebx+0xBE8]",
            "addss xmm1, [0x5AFFFC]",
            "subss xmm0, [ebx+0xBE8]",
            "divss xmm0, xmm1",
            "jmp exit",

            "linear:",
            "movss xmm1, [0x5C08FC]",
            $"subss xmm1, [{(int)(&ptr->LinearSpeedCapOverride)}]",
            "addss xmm1, [0x5AFFFC]",
            "subss xmm0, [ebx+0xBE8]",
            "divss xmm0, xmm1",
            "jmp exit",

            "exit:",

        }, 0x004BAAE2, AsmHookBehaviour.DoNotExecuteOriginal, 36).Activate();
    }
}
