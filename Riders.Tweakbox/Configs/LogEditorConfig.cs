using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Configs;

public class LogEditorConfig : JsonConfigBase<LogEditorConfig, LogEditorConfig.Internal>
{
    public class Internal
    {
        public LogCategory Console = Log.DefaultConsoleCategories;
        public LogCategory Hud = Log.DefaultHudCategories;
        public Pivots.Pivot Position = Pivots.Pivot.BottomLeft;

        public void Apply()
        {
            Log.HudCategories = Hud;
            Log.ConsoleCategories = Console;
            Shell.LogPosition = Position;
        }
    }

    /// <inheritdoc />
    public override void Apply()
    {
        base.Apply();
        Data.Apply();
    }
}
