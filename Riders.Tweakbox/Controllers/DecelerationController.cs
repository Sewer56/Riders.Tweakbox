using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Interfaces.Internal;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Implementation of <see cref="DecelProperties"/> in x86 SSE assembly.
/// </summary>
public unsafe class DecelerationController : IController
{
    private IAsmHook _decelAsmHook;

    public unsafe DecelerationController(IReloadedHooks hooks)
    {
        var utilities = hooks.Utilities;

        _decelAsmHook = hooks.CreateAsmHook(new[]
        {
            "use32",

            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push ebx", // Player
            $"{utilities.AssembleAbsoluteCall<ModifyDecelerationFnPtr>(typeof(DecelerationController), nameof(CalculateDecelerationHook), false)}",
            $"{utilities.PopFromX87ToXmm()}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x004BAAE2, AsmHookBehaviour.DoNotExecuteOriginal, 36).Activate();
    }

    [UnmanagedCallersOnly]
    private static float CalculateDecelerationHook(Player* player)
    {
        ref var decelProps = ref Static.DecelProperties;
        if (decelProps.Mode == DecelMode.Linear)
            return CalculateDecelerationLinear(player, ref decelProps);

        return CalculateDecelerationDefault(player);
    }

    private static float CalculateDecelerationDefault(Player* player)
    {
        // Vanilla implementation.
        float const260     = *(float*)0x5C08FC;
        float constEpsilon = *(float*)0x5AFFFC;

        var numerator   = player->Speed - player->SpeedCap;
        var denominator = (const260 - player->SpeedCap) + constEpsilon;
        return numerator / denominator;
    }

    private static float CalculateDecelerationLinear(Player* player, ref DecelProperties props)
    {
        // DX Style Linear
        float const260     = *(float*)0x5C08FC;
        float constEpsilon = *(float*)0x5AFFFC;

        var numerator   = player->Speed - props.LinearSpeedCapOverride;
        if (numerator > props.LinearMaxSpeedOverCap)
            numerator = props.LinearMaxSpeedOverCap;

        var denominator = (const260 - player->SpeedCap) + constEpsilon;
        var result = numerator / denominator;

        return result;
    }

    [Function(CallingConventions.Stdcall)]
    public struct ModifyDecelerationFnPtr { public FuncPtr<BlittablePointer<Player>, float> Value; }
}
