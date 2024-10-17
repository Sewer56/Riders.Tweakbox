using System;
using DearImguiSharp;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiWindowFlags;
namespace Riders.Tweakbox.Misc;

/// <summary>
/// Renders the first time welcome screen.
/// </summary>
public class WelcomeScreenRenderer : IDisposable
{
    private const string Title = "Welcome!";

    private HorizontalCenterHelper _centerHelper = new HorizontalCenterHelper();
    private ImVec4 _accentColor = Utilities.HexToFloat(0xef5350ff);
    private uint _hyperlinkColor = 0x42a5f5ff;

    ~WelcomeScreenRenderer() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _accentColor?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Pass this function to <see cref="Shell.AddCustom"/>.
    /// </summary>
    public unsafe bool RenderFirstTimeDialog()
    {
        bool isOpened = true;

        ImGui.OpenPopupStr(Title, (int)ImGuiPopupFlags.NoOpenOverExistingPopup);
        ImGui.__Internal.SetNextWindowSize(new ImVec2.__Internal() { x = 400, y = 0 }, (int)ImGuiCond.Always);
        if (ImGui.BeginPopupModal(Title, ref isOpened, (int)(AlwaysAutoResize | NoTitleBar | NoSavedSettings)))
        {
            // Title
            ImGui.TextColored(_accentColor, "Ohayou !");
            ImGui.Spacing();

            // Text
            ImGui.TextWrapped("This is probably your first time using Tweakbox.");
            ImGui.Spacing();

            ImGui.TextWrapped("Please note that this project is an active work in progress (alpha build). Features like Netplay can be incomplete, buggy and prone to crashing.");
            ImGui.Spacing();

            Hyperlink.CreateText("Please report crashes and issues using the guidelines provided in the documentation.", "https://sewer56.dev/Riders.Tweakbox/", true, _hyperlinkColor);

            // Footer
            ImGui.Spacing();
            ImGui.Text("And most of all, remember to ");
            ImGui.SameLine(0, 0);
            ImGui.TextColored(_accentColor, "have fun!");

            // Render OK Button
            ImGui.Spacing();
            _centerHelper.Begin();
            if (ImGui.Button("OK", Sewer56.Imgui.Misc.Constants.ButtonSizeThin))
                isOpened = false;

            _centerHelper.End();
            ImGui.EndPopup();
        }

        if (!isOpened)
            Dispose();

        return isOpened;
    }
}
