using System;
namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;

/// <summary>
/// The type of cheat that was detected.
/// </summary>
[Flags]
public enum CheatKind : byte
{
    None = 0,
    Speedhack = 1 << 0,
    Teleport = 1 << 1,
    ModifiedStats = 1 << 2,
    LapCounter = 1 << 3,
    DriftFrameCounter = 1 << 4,
    BoostFrameCounter = 1 << 5,

    // TODO: Implement after initial release
    RngManipulation = 1 << 6, // Call rand, then again, check for same value. (Then restore)
}
