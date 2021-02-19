using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared
{
    [Flags]
    public enum MovementFlags : ushort
    {
        None,
        Braking         = 1 << 0,
        ChargingJump    = 1 << 1,
        Drifting        = 1 << 2,
        AttachToRail    = 1 << 3,
    }
}