using System.Collections.Generic;
using SharpDX.Direct3D9;

namespace Riders.Tweakbox.Misc.Extensions
{
    public static class D3DExtensions
    {
        /// <summary>
        /// Compares the given display mode to another mode.
        /// </summary>
        public static bool ResolutionEqual(this DisplayMode me, DisplayMode other)
        {
            return me.Height == other.Height
                   && me.Width == other.Width;
        }

        /// <summary>
        /// Compares the given display mode to another mode.
        /// </summary>
        public static bool Equal(this DisplayMode me, DisplayMode other)
        {
            return me.Format == other.Format
                   && me.Height == other.Height
                   && me.Width == other.Width
                   && me.RefreshRate == other.RefreshRate;
        }

        /// <summary>
        /// Converts the current mode to a human readable string.
        /// </summary>
        public static string AsString(this DisplayMode me)
        {
            if (me.RefreshRate == 0)
                return $"{me.Width}x{me.Height}, Auto Hz";
            
            return $"{me.Width}x{me.Height}, {me.RefreshRate}Hz";
        }

        /// <summary>
        /// Converts the current mode to a human readable string.
        /// </summary>
        public static List<string> AsStrings(this DisplayModeCollection me)
        {
            var strings = new List<string>(me.Count);
            foreach (var item in me)
                strings.Add(item.AsString());

            return strings;
        }
    }
}