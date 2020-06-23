using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared
{
    [Flags]
    public enum MovementFlags : ushort
    {
        None,
        Boost           = 1,
        Tornado         = 1 << 1,
        Braking         = 1 << 2,
        ChargingJump    = 1 << 3,
        Drifting        = 1 << 4,
        AttachToRail    = 1 << 5,
        Left            = 1 << 6,
        Right           = 1 << 7,
        Up              = 1 << 8,
        Down            = 1 << 9,

    }
}