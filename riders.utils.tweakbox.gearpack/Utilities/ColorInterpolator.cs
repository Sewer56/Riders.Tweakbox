using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colourful;

namespace Riders.Tweakbox.Gearpack.Utilities
{
    public static class ColorInterpolator
    {
        /// <summary>
        /// Retrieves a colour of the rainbow with a specified chroma and lightness.
        /// </summary>
        /// <param name="chroma">Range 0 to 100. The quality of a color's purity, intensity or saturation. </param>
        /// <param name="lightness">Range 0 to 100. The quality (chroma) lightness or darkness.</param>
        /// <param name="time">A normalized time between 0-1 which dictates the hue of the colour. The hue ranges between 0 to 360 and is calculated by time * 360.</param>
        public static LChabColor GetRainbowColor(double chroma, double lightness, double time) => new LChabColor(lightness, chroma, time * 360.0F);
    }
}
