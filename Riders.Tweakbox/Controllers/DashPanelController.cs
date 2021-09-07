using System;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Types;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Interfaces.Internal;

namespace Riders.Tweakbox.Controllers;

public class DashPanelController : IController
{
    public EventController Controller = IoC.GetSingleton<EventController>(); // Load Order

    public unsafe DashPanelController()
    {
        EventController.SetDashPanelSpeed += SetDashPanelSpeed;
    }

    private unsafe IntFloat SetDashPanelSpeed(Player* player, float targetSpeed)
    {
        ref var props = ref Static.PanelProperties;

        switch (props.Mode)
        {
            case DashPanelMode.Vanilla:
                return targetSpeed;
            case DashPanelMode.Fixed:
                return props.FixedSpeed;
            case DashPanelMode.Additive:
                return player->Speed + props.AdditiveSpeed;
            case DashPanelMode.Multiplicative:
                return player->Speed * (1 + props.MultiplicativeSpeed);
            case DashPanelMode.MultiplyOrFixed:
                var multiplied = player->Speed * (1 + props.MultiplicativeSpeed);
                return Math.Max(multiplied, props.MultiplicativeMinSpeed);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
