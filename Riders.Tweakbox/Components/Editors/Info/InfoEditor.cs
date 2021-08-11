using System;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Shell.Interfaces;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Editors.Info
{
    public class InfoEditor : ComponentBase<InfoEditorConfig>, IComponent
    {
        public override string Name { get; set; } = "Info Editor";

        public InfoEditor(IO io) : base(io, io.InfoConfigFolder, io.GetInfoConfigFiles)
        {

        }

        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();
                RenderInfoEditor();
            }

            ImGui.End();
        }

        private void RenderInfoEditor()
        {
            var data = Config.Data;
            var showPercent = "Show %";
            var showNumber = "Show Number";
            var showGraph = "Show Graph";

            Reflection.MakeControlEnum(ref data.Position, "Position").Notify(data, nameof(data.Position));
            ImGui.DragInt("Window Width", ref data.Width, 1, 0, 16384, null, 0).Notify(data, nameof(data.Width));
            ImGui.DragInt("Window Height", ref data.Height, 1, 0, 16384, null, 0).Notify(data, nameof(data.Height));

            ImGui.DragInt("Graph Width", ref data.GraphWidth, 1, 0, 16384, null, 0).Notify(data, nameof(data.GraphWidth));
            ImGui.DragInt("Graph Height", ref data.GraphHeight, 1, 0, 16384, null, 0).Notify(data, nameof(data.GraphHeight));

            if (ImGui.TreeNodeStr("CPU Usage"))
            {
                Reflection.MakeControl(ref data.ShowCpuNumber, showPercent).Notify(data, nameof(data.ShowCpuNumber));
                Reflection.MakeControl(ref data.ShowCpuGraph, showGraph).Notify(data, nameof(data.ShowCpuGraph));
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("FPS"))
            {
                Reflection.MakeControl(ref data.ShowFpsNumber, showNumber).Notify(data, nameof(data.ShowFpsNumber));
                Reflection.MakeControl(ref data.ShowFpsGraph, showGraph).Notify(data, nameof(data.ShowFpsGraph));
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Frame Time"))
            {
                Reflection.MakeControl(ref data.ShowFrameTimeNumber, showNumber).Notify(data, nameof(data.ShowFrameTimeNumber));
                Reflection.MakeControl(ref data.ShowFrameTimeGraph, showGraph).Notify(data, nameof(data.ShowFrameTimeGraph));
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Render Time"))
            {
                ImGui.TextWrapped("Shows the amount of time taken to process an individual frame (i.e. non-idle time).");
                Reflection.MakeControl(ref data.ShowRenderTimeNumber, showNumber).Notify(data, nameof(data.ShowRenderTimeNumber));
                Reflection.MakeControl(ref data.ShowRenderTimeGraph, showGraph).Notify(data, nameof(data.ShowRenderTimeGraph));
                Reflection.MakeControl(ref data.ShowRenderTimePercent, showPercent).Notify(data, nameof(data.ShowRenderTimePercent));
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Potential FPS"))
            {
                ImGui.TextWrapped("Shows an estimate of the achievable FPS should the framerate be uncapped.");
                Reflection.MakeControl(ref data.ShowMaxFpsNumber, showNumber).Notify(data, nameof(data.ShowMaxFpsNumber));
                Reflection.MakeControl(ref data.ShowMaxFpsGraph, showGraph).Notify(data, nameof(data.ShowMaxFpsGraph));
                ImGui.TreePop();
            }
        }
    }
}
