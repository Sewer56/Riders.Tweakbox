using System;
using Reloaded.Memory.Interop;
using Riders.Tweakbox.Shared.Structs;

namespace Riders.Tweakbox.Shared
{
    public static class Static
    {
        /// <summary>
        /// The current Dash Panel settings.
        /// </summary>
        public static DashPanelProperties PanelProperties = DashPanelProperties.Default();

        /// <summary>
        /// The current acceleration settings.sss
        /// </summary>
        public static Pinnable<DecelProperties> DecelProperties = new Pinnable<DecelProperties>(Structs.DecelProperties.GetDefault());
    }
}
