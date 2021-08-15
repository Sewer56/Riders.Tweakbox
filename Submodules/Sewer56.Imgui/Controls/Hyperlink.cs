using System.Diagnostics;
using DearImguiSharp;
namespace Sewer56.Imgui.Controls;

public static class Hyperlink
{
    /// <summary>
    /// Displays a tooltip if the last item was hovered over.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="url">The URL to launch.</param>
    /// <param name="colour">The colour of the hyperlink (RGBA).</param>
    public static void CreateText(string text, string url, uint colour = 0x42a5f5ff)
    {
        ImGui.__Internal.PushStyleColorVec4((int)ImGuiCol.ImGuiColText, Utilities.Utilities.HexToFloatInternal(colour));
        ImGui.TextWrapped(text);
        if (ImGui.IsItemClicked((int)ImGuiMouseButton.ImGuiMouseButtonLeft))
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

        ImGui.PopStyleColor(1);
    }

}
