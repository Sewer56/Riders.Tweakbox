using System;

namespace Riders.Tweakbox.Interfaces.Structs.Enums;

[Flags]
public enum PlayerStateFlags : int
{
    None = 1 << 0,

    /// <summary>
    /// The running state that is applied at the start of a race.
    /// </summary>
    Running = 1 << 1,

    /// <summary>
    /// Resets the player as if they were to fall out of map, go the wrong way, etc.
    /// </summary>
    Reset = 1 << 3,

    /// <summary>
    /// Brings up the Retire Screen as if the current mission were to be failed by the player.
    /// </summary>
    Retire = 1 << 4,

    /// <summary>
    /// Normally driving forward on extreme gear/skates/bike.
    /// </summary>
    NormalOnBoard = 1 << 5,

    /// <summary>
    /// Triggers a character jump.
    /// </summary>
    Jump = 1 << 6,

    /// <summary>
    /// Applied when the player falls off a cliff/ledge without jumping.
    /// </summary>
    FreeFalling = 1 << 7,

    /// <summary>
    /// Doing Tricks (Horizontal Ramp) e.g. First Jump Metal City
    /// </summary>
    TrickJumpHorizontal = 1 << 8,

    /// <summary>
    /// Doing Tricks (Vertical Ramp) e.g. First Jump Ice Factory
    /// </summary>
    TrickJumpVertical = 1 << 9,

    TrickJumpUnknown1 = 1 << 0x0A,
    TrickJumpUnknown2 = 1 << 0x0B,

    /// <summary>
    /// Doing Tricks (Flat Vertical Ramp) (e.g. Ice Factory 2nd Jump), first jump after
    /// Metal City's first turn.
    /// </summary>
    TrickJumpFlatVertical = 1 << 0x0C,

    /// <summary>
    /// Doing Tricks (Turbulence) e.g. Turbulence
    /// </summary>
    TrickJumpTurbulence = 1 << 0x0D,

    /// <summary>
    /// Turbulence
    /// </summary>
    Turbulence = 1 << 0x10,

    /// <summary>
    /// Inside an Auto/Rotate Stick Section (or arrows on PC version).
    /// (Setting manually = crash, needs rail set somewhere first)
    /// </summary>
    RotateSection = 1 << 0x11,

    /// <summary>
    /// Grinding (Setting manually = crash, needs rail set somewhere first)
    /// </summary>
    Grinding = 1 << 0x12,

    /// <summary>
    /// Flying (Flight Formation).
    /// </summary>
    Flying = 1 << 0x13,

    /// <summary>
    /// Attacking an enemy/rival.
    /// (Setting manually = crash, needs enemy set somewhere first)
    /// </summary>
    Attacking = 1 << 0x15,

    /// <summary>
    /// Getting attacked by an enemy/rival.
    /// (Setting manually = crash, needs enemy set somewhere first)
    /// </summary>
    GettingAttacked = 1 << 0x16,

    /// <summary>
    /// Running state after the player crosses the start line.
    /// </summary>
    RunningAfterStart = 1 << 0x19,

    /// <summary>
    /// Triggers the electric shock encountered if the player passes the start
    /// line too early.
    /// </summary>
    ElectricShock = 1 << 0x1A,

    /// <summary>
    /// Purpose unknown, brings player to a halt.
    /// </summary>
    InstantStop = 1 << 0x1B,

    /// <summary>
    /// ElectricShock but longer.
    /// </summary>
    ElectricShockLong = 1 << 0x1C,

    /// <summary>
    /// Some variant of ElectricShock which crashes the game.
    /// </summary>
    ElectricShockCrash = 1 << 0x1D,
}

public static class PlayerStateFlagsExtensions
{
    /// <summary>
    /// Checks if the given set of flags contains a specific type.
    /// </summary>
    /// <param name="flags">The flags to check.</param>
    /// <param name="state">The type to check for.</param>
    public static bool ContainsState(this PlayerStateFlags flags, int state)
    {
        var flag = 1 << ((int)state - 1);
        return ((int)flags & flag) != 0;
    }
}