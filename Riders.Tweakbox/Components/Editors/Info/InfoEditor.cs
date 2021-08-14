using System;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
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
            if (ImGui.Button("Add", Constants.ButtonSize))
                data.Widgets.Add(new InfoEditorConfig.WidgetConfig());

            for (var x = data.Widgets.Count - 1; x >= 0; x--)
            {
                if (ImGui.CollapsingHeaderTreeNodeFlags($"Widget {x}", 0))
                {
                    ImGui.PushID_Int(x);
                    if (!RenderWidgetEditor(data.Widgets[x]))
                        data.Widgets.RemoveAt(x);

                    ImGui.PopID();
                }
            }
        }

        private bool RenderWidgetEditor(InfoEditorConfig.WidgetConfig data)
        {
            var showPercent = "Show %";
            var showNumber  = "Show Number";
            var showGraph   = "Show Graph";

            if (ImGui.TreeNodeStr("Position & Style"))
            {
                Reflection.MakeControlEnum(ref data.Position, "Position");
                ImGui.DragInt("Window Width", ref data.Width, 1, 0, 16384, null, 0);
                ImGui.DragInt("Window Height", ref data.Height, 1, 0, 16384, null, 0);

                ImGui.DragInt("Graph Width", ref data.GraphWidth, 1, 0, 16384, null, 0);
                ImGui.DragInt("Graph Height", ref data.GraphHeight, 1, 0, 16384, null, 0);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("CPU Usage"))
            {
                Reflection.MakeControl(ref data.ShowCpuNumber, showPercent);
                Reflection.MakeControl(ref data.ShowCpuGraph, showGraph);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("FPS"))
            {
                Reflection.MakeControl(ref data.ShowFpsNumber, showNumber);
                Reflection.MakeControl(ref data.ShowFpsGraph, showGraph);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Frame Time"))
            {
                Reflection.MakeControl(ref data.ShowFrameTimeNumber, showNumber);
                Reflection.MakeControl(ref data.ShowFrameTimeGraph, showGraph);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Render Time"))
            {
                ImGui.TextWrapped("Shows the amount of time taken to process an individual frame (i.e. non-idle time).");
                Reflection.MakeControl(ref data.ShowRenderTimeNumber, showNumber);
                Reflection.MakeControl(ref data.ShowRenderTimeGraph, showGraph);
                Reflection.MakeControl(ref data.ShowRenderTimePercent, showPercent);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Potential FPS"))
            {
                ImGui.TextWrapped("Shows an estimate of the achievable FPS should the framerate be uncapped.");
                Reflection.MakeControl(ref data.ShowMaxFpsNumber, showNumber);
                Reflection.MakeControl(ref data.ShowMaxFpsGraph, showGraph);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Memory Heap"))
            {
                ImGui.TextWrapped("Shows the amount of memory used in the game's internal heap.");
                Reflection.MakeControl(ref data.ShowHeapNumber, showNumber);
                Reflection.MakeControl(ref data.ShowHeapGraph, showGraph);
                Reflection.MakeControl(ref data.ShowHeapPercent, showPercent);
                ImGui.TreePop();
            }

            if (ImGui.Button("Delete", Constants.ButtonSize))
                return false;

            return true;
        }
    }
}
