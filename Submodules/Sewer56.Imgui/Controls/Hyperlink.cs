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
    /// <param name="isWrapped">Whether to wrap the text or not.</param>
    public static void CreateText(string text, string url, bool isWrapped = true, uint colour = 0x42a5f5ff)
    {
        ImGui.__Internal.PushStyleColorVec4((int)ImGuiCol.ImGuiColText, Utilities.Utilities.HexToFloatInternal(colour));
        if (isWrapped)
            ImGui.TextWrapped(text);
        else
            ImGui.Text(text);
        
        if (ImGui.IsItemClicked((int)ImGuiMouseButton.ImGuiMouseButtonLeft))
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

        ImGui.PopStyleColor(1);
    }

}
