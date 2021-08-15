using System.Runtime.CompilerServices;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Reflection = Sewer56.Imgui.Controls.Reflection;
namespace Riders.Tweakbox.Components.Debug.Log;

public class LogWindow : ComponentBase<LogEditorConfig>
{
    /// <inheritdoc />
    public override string Name { get; set; } = "Log Configuration";

    public LogWindow(IO io) : base(io, io.LogConfigFolder, io.GetLogsConfigFiles, IO.JsonConfigExtension)
    {

    }

    /// <inheritdoc />
    public override unsafe void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            ProfileSelector.Render();
            ImGui.PushID_Int(0);
            ImGui.TextWrapped("UI/HUD Enabled Categories");
            Reflection.MakeControlEnum((LogCategory*)Unsafe.AsPointer(ref Config.Data.Hud), null);
            ImGui.PopID();

            ImGui.PushID_Int(1);
            ImGui.TextWrapped("Console & Log File Enabled Categories");
            Reflection.MakeControlEnum((LogCategory*)Unsafe.AsPointer(ref Config.Data.Console), null);
            ImGui.PopID();
            Config.Data.Apply();
        }

        ImGui.End();
    }
}
