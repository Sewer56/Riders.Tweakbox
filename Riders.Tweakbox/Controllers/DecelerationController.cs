using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Interfaces.Internal;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Implementation of <see cref="DecelProperties"/> in x86 SSE assembly.
/// </summary>
public unsafe class DecelerationController : IController
{
    public unsafe DecelerationController(IReloadedHooks hooks)
    {
        EventController.SetDeceleration += SetDeceleration;
    }

    private void SetDeceleration(ref float value, Player* player) => value = CalculateDecelerationTweakbox(player);

    private static float CalculateDecelerationTweakbox(Player* player)
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
        if (props.EnableMaxSpeedOverCap && numerator > props.LinearMaxSpeedOverCap)
            numerator = props.LinearMaxSpeedOverCap;

        var denominator = (const260 - player->SpeedCap) + constEpsilon;
        var result = numerator / denominator;

        return result;
    }
}
