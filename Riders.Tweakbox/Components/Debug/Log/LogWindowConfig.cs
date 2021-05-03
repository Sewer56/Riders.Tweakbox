using Riders.Tweakbox.Components.Common;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindowConfig : JsonConfigBase<LogWindowConfig, LogWindowConfig.Internal>
    {
        public class Internal
        {
            public LogCategory Console = Misc.Log.DefaultConsoleCategories;
            public LogCategory Hud     = Misc.Log.DefaultHudCategories;

            public void Apply()
            {
                Misc.Log.HudCategories = Hud;
                Misc.Log.ConsoleCategories = Console;
            }
        }

        /// <inheritdoc />
        public override void Apply()
        {
            base.Apply();
            Data.Apply();
        }
    }
}
