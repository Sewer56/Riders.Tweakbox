using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Shared
{
    [Flags]
    public enum AttackModes : byte
    {
        None,
        Boost   = 1,
        Tornado = 1 << 1,
        Attack  = 1 << 2
    }
}