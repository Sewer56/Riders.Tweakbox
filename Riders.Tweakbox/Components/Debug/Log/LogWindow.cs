using System.Runtime.CompilerServices;
using DearImguiSharp;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindow : ComponentBase<LogWindowConfig>
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Log Configuration";

        public LogWindow(IO io) : base(io, io.LogConfigFolder, io.GetLogsConfigFiles)
        {
            
        }

        /// <inheritdoc />
        public override unsafe void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();
                Reflection.MakeControlEnum((LogCategory*) Unsafe.AsPointer(ref Misc.Log.EnabledCategories), "Enabled Categories");
            }

            ImGui.End();
        }
    }
}
