using System;
using EnumsNET;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Netplay.Messages.Unreliable.Enums
{
    /// <summary>
    /// Minimal version of <see cref="PlayerControlFlags"/>
    /// </summary>
    [Flags]
    public enum MinControlFlags
    {
        None            = 0,
        IsFloored       = 1 << 0,
        Falling         = 1 << 1,
        Grass           = 1 << 2,
        NoHoverAndTrail = 1 << 3,
    }

    public static class MinControlFlagsFunctions
    {
        /// <summary>
        /// Extracts the <see cref="MinControlFlags"/> from the current set of flags.
        /// </summary>
        public static MinControlFlags Extract(this ref PlayerControlFlags flags)
        {
            var result = MinControlFlags.None;
            if (flags.HasAnyFlags(PlayerControlFlags.Falling))
                result |= MinControlFlags.Falling;

            if (flags.HasAnyFlags(PlayerControlFlags.Grass))
                result |= MinControlFlags.Grass;

            if (flags.HasAnyFlags(PlayerControlFlags.NoHoverAndTrail))
                result |= MinControlFlags.NoHoverAndTrail;

            if (flags.HasAnyFlags(PlayerControlFlags.IsFloored))
                result |= MinControlFlags.IsFloored;

            return result;
        }

        /// <summary>
        /// Merges the <see cref="MinControlFlags"/> into the current set of flags.
        /// </summary>
        public static void SetMinFlags(this ref PlayerControlFlags flags, in MinControlFlags value)
        {
            if (value.HasAnyFlags(MinControlFlags.Falling))
                flags |= PlayerControlFlags.Falling;
            else
                flags &= ~PlayerControlFlags.Falling;

            if (value.HasAnyFlags(MinControlFlags.Grass))
                flags |= PlayerControlFlags.Grass;
            else
                flags &= ~PlayerControlFlags.Grass;

            if (value.HasAnyFlags(MinControlFlags.IsFloored))
                flags |= PlayerControlFlags.IsFloored;
            else
                flags &= ~PlayerControlFlags.IsFloored;

            if (value.HasAnyFlags(MinControlFlags.NoHoverAndTrail))
                flags |= PlayerControlFlags.NoHoverAndTrail;
            else
                flags &= ~PlayerControlFlags.NoHoverAndTrail;
        }
    }
}