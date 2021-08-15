using System;
namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;

/// <summary>
/// Decides if boost or tornado.
/// </summary>
[Flags]
public enum BoostTornadoFlags
{
    None = 0,
    Boost = 1 << 0,
    Tornado = 1 << 1
}
