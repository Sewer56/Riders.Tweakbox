using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Configs
{
    public class LogEditorConfig : JsonConfigBase<LogEditorConfig, LogEditorConfig.Internal>
    {
        public class Internal
        {
            public LogCategory Console = Log.DefaultConsoleCategories;
            public LogCategory Hud     = Log.DefaultHudCategories;

            public void Apply()
            {
                Log.HudCategories = Hud;
                Log.ConsoleCategories = Console;
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
