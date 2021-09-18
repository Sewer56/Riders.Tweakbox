using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Components.Debug;

public class SlipstreamDebug : ComponentBase
{
    public override string Name { get; set; } = "Slipstream Debug";
    private GameModifiersController _modifiersController;

    public SlipstreamDebug(GameModifiersController controller)
    {
        _modifiersController = controller;
    }

    public unsafe override void Render()
    {
        ref var slipStream = ref _modifiersController.Slipstream;
        if (ImGui.Begin(Name, ref Enabled, 0))
        {
            slipStream.AlwaysCalculateSlipstream = true;

            // Set font
            using var originalFont = ImGui.GetFont();
            ImGui.SetCurrentFont(Shell.MonoFont);

            // Render Debug Info
            var maxPlayers = *State.NumberOfRacers;
            for (int x = 0; x < maxPlayers; x++)
            {
                if (ImGui.TreeNodeStr($"Player {x}"))
                {
                    ImGui.TextWrapped($"Total Slip Power: {slipStream.TotalSlipPower[x]:00.0000000}");
                    for (int y = 0; y < maxPlayers; y++)
                    {
                        var info = slipStream.SlipstreamDebugInformation[x, y];
                        ImGui.TextWrapped($"From Player {y} | Angle: {info.Angle:00.000}, Alignment: {info.Alignment:00.000}, Dist: {info.Distance:00.000}, Power: {info.SlipPower:0.0000000}, AnglePower: {info.SlipAnglePower:0.000}, AlignPower: {info.SlipAlignmentPower:0.000} DistMult: {info.SlipDistanceMult:0.0000000}");
                    }

                    ImGui.TreePop();
                }
            }

            // Restore Font
            ImGui.SetCurrentFont(originalFont);
        }
        else
        {
            slipStream.AlwaysCalculateSlipstream = false;
        }

        ImGui.End();
    }
}
