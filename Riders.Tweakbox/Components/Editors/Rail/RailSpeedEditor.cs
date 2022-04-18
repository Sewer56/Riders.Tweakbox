using System;
using System.IO;
using System.Windows.Media.Animation;
using DearImguiSharp;
using Microsoft.Win32;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Configs.Misc;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Utility;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Editors.Rail;

public class RailSpeedEditor : ComponentBase, IComponent
{
    public override string Name { get; set; } = "Rail Speed Editor";

    private RailController _controller = IoC.GetSingleton<RailController>();
    private NetplayController _netplayController = IoC.Get<NetplayController>();
    private ImVec2 _graphSize = new ImVec2() { X = 0, Y = 70 };

    public bool IsAvailable() => !_netplayController.IsConnected();

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            RenderInternal();
            ImGui.End();
        }
    }

    private void RenderInternal()
    {
        ref var rails = ref _controller.Configuration.Data;
        ImGui.PushItemWidth(ImGui.GetFontSize() * -12);
        if (rails.Rails.Count == 0)
        {
            ImGui.Text("No rails are currently loaded.");
        }
        else
        {
            for (var x = 0; x < rails.Rails.Count; x++)
            {
                var rail = rails.Rails[x];
                if (ImGui.CollapsingHeaderTreeNodeFlags($"Rail {x}", 0))
                {
                    ImGui.PushID_Int(x);
                    RenderRailEntry(rail, x);
                    ImGui.PopID();
                }
            }
        }

        if (ImGui.Button("Import from File", Constants.Zero))
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Load From Path";
            openFileDialog.FileName = "Rails.json";
            var result = openFileDialog.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(openFileDialog.FileName))
                _controller.Configuration.FromBytes(File.ReadAllBytes(openFileDialog.FileName));
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Export to File", Constants.Zero))
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save to Path";
            saveFileDialog.FileName = "Rails.json";
            var result = saveFileDialog.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                File.WriteAllBytes(saveFileDialog.FileName, _controller.Configuration.ToBytes());
        }

        ImGui.PopItemWidth();
    }

    private unsafe void RenderRailEntry(CustomRailConfiguration.RailEntry rail, int railIndex)
    {
        ImGui.Checkbox("Is Enabled", ref rail.IsEnabled);
        Tooltip.TextOnHover("If unchecked, custom rail behaviour is not applied.");

        Reflection.MakeControl(ref rail.Frames, "Number of Frames", 0.1f, 0, int.MaxValue);
        Tooltip.TextOnHover("Number of frames between minimum and maximum speed.\n" +
                            "If this is 0, Min Speed will be used.");

        Reflection.MakeControlEnum(ref rail.EasingSetting, "Easing Setting"); 
        Tooltip.TextOnHover("Controls the post processing algorithm used to scale between Minimum and Maximum Speed.\n" +
            "These algorithms are listed in increasing levels of growth rate.\n" +
            "Don't know what this means? Google \"Easing Functions\"");

        Reflection.MakeControlEnum(ref rail.EasingMode, "Ease Mode");
        Tooltip.TextOnHover("Quick Reference:\n" +
                            "- Ease In produces smooth upward slope.\n" +
                            "- Ease Out produces 1 - EaseIn.\n" +
                            "- Ease In/Out produces smooth upward slope for first half and opposite for second half.");

        Reflection.MakeControl(ref rail.SpeedCapInitial, "Min Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(rail.SpeedCapInitial)})");
        Tooltip.TextOnHover("Initial speed cap to assign to the rail.");

        Reflection.MakeControl(ref rail.SpeedCapEnd, "Max Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(rail.SpeedCapEnd)})");
        Tooltip.TextOnHover("Maximum speed cap to assign to the rail.");

        if (ImGui.Button("Teleport to Rail", Constants.ButtonSize))
            _controller.TeleportToRail(railIndex, Player.Players[0].AsPointer());

        // Render Graph
        if (rail.Frames > 1)
        {
            Span<float> values = stackalloc float[rail.Frames];
            for (int x = 0; x < values.Length; x++)
                values[x] = rail.CalculateSpeed(x);

            ImGui.PlotLinesFloatPtr("Speed", ref values[0], rail.Frames, 0, null, values[0], values[^1], _graphSize, sizeof(float));
        }
    }
}