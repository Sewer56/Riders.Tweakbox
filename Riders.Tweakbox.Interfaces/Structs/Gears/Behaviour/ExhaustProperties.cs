using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour
{
    public struct ExhaustProperties
    {
        public bool Enabled = false;

        /// <summary>
        /// Overrides the colour used for the exhaust trail.
        /// </summary>
        public SetValueHandler<System.Drawing.Color> GetExhaustTrailColour;
    }
}