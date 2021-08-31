using DearImguiSharp;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.Imgui.Utilities;
using System;
using System.Collections.Generic;
using static DearImguiSharp.ImGuiWindowFlags;

namespace Riders.Tweakbox.Components.Main;

public class AboutMenu : ComponentBase, IComponent
{
    public override string Name { get; set; } = "About Tweakbox";
    private InformationWindow _informationWindow = new InformationWindow("About Menu", Pivots.Pivot.Center, Pivots.Pivot.Center, false);
    private string _tweakboxVersion = typeof(Program).Assembly.GetName().Version.ToString(3);

    private List<HorizontalCenterHelper> _centerHelpers = new List<HorizontalCenterHelper>();

    public AboutMenu()
    {
        _informationWindow.WindowFlags &= ~ImGuiWindowFlagsNoInputs;
    }

    public override void Render()
    {
        if (!Enabled)
            return;

        _informationWindow.Begin();
        RenderContents();
        _informationWindow.End();
    }

    private void RenderContents()
    {
        int centerIndex = 0;
        
        RenderCentered(ref centerIndex, () => ImGui.Text($"Riders Tweakbox | {_tweakboxVersion}"));
        RenderCentered(ref centerIndex, () => ImGui.Text($"A Product of Sewer56"));
        ImGui.Separator();

        RenderCenteredLabeledLink(ref centerIndex, "Wiki", "sewer56.dev/Riders.Tweakbox/", "https://sewer56.dev/Riders.Tweakbox/");
        RenderCenteredLabeledLink(ref centerIndex, "Source Code", "github.com/Sewer56/Riders.Tweakbox/", "https://github.com/Sewer56/Riders.Tweakbox/");
        ImGui.Separator();
        RenderCentered(ref centerIndex, () => ImGui.Text($"Useful Links"));
        RenderCenteredLabeledLink(ref centerIndex, "Discord", "Extreme Gear Labs", "https://discord.com/invite/eCuQeaN");
        RenderCenteredLabeledLink(ref centerIndex, "Find Other Mods", "GameBanana", "https://gamebanana.com/games/6355");

        RenderCentered(ref centerIndex, () =>
        {
            if (ImGui.Button("See Ya Later!", Constants.Zero))
                Enabled = false;
        });
    }

    private void RenderCenteredLabeledLink(ref int centerIndex, string linkLabel, string urlLabel, string url)
    {
        RenderCentered(ref centerIndex, () =>
        {
            ImGui.BeginGroup();
            ImGui.Text($"{linkLabel}: ");
            ImGui.SameLine(0, 0);
            Hyperlink.CreateText(urlLabel, url, false);
            ImGui.EndGroup();
        });
    }

    private void RenderCentered(ref int index, Action render)
    {
        var neededHelpers = index - _centerHelpers.Count;
        for (int x = 0; x <= neededHelpers; x++)
            _centerHelpers.Add(new HorizontalCenterHelper());

        var helper = _centerHelpers[index];
        helper.Begin();
        render();
        helper.End();
        index++;
    }
}
