using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Types;
using Riders.Tweakbox.Shared;
using Riders.Tweakbox.Shared.Structs;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Controllers
{
    public class DashPanelController : IController
    {
        public EventController Controller = IoC.GetSingleton<EventController>();

        public unsafe DashPanelController()
        {
            Controller.SetDashPanelSpeed += SetDashPanelSpeed;
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
}
