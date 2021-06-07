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
    public class SpeedShoesController : IController
    {
        public EventController Controller = IoC.GetSingleton<EventController>();

        public unsafe SpeedShoesController()
        {
            Controller.SetSpeedShoesSpeed += SetSpeedShoesSpeed;
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
}
