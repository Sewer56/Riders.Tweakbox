using System;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Types;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Interfaces.Internal;

namespace Riders.Tweakbox.Controllers;

public class SpeedShoesController : IController
{
    public EventController Controller = IoC.GetSingleton<EventController>(); // Ensure load order

    public unsafe SpeedShoesController()
    {
        EventController.SetSpeedShoesSpeed += SetSpeedShoesSpeed;
    }

    private unsafe IntFloat SetSpeedShoesSpeed(Player* player, float targetSpeed)
    {
        ref var props = ref Static.SpeedShoeProperties;

        switch (props.Mode)
        {
            case SpeedShoesMode.Vanilla:
                return targetSpeed;
            case SpeedShoesMode.Fixed:
                return props.FixedSpeed;
            case SpeedShoesMode.Additive:
                return player->Speed + props.AdditiveSpeed;
            case SpeedShoesMode.Multiplicative:
                return player->Speed * (1 + props.MultiplicativeSpeed);
            case SpeedShoesMode.MultiplyOrFixed:
                var multiplied = player->Speed * (1 + props.MultiplicativeSpeed);
                return Math.Max(multiplied, props.MultiplicativeMinSpeed);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
