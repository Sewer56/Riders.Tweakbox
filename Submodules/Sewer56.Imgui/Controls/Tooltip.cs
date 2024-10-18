using DearImguiSharp;
namespace Sewer56.Imgui.Controls;

public static class Tooltip
{
    // TODO: Optimise this with interpolated string handler. 
    
    /// <summary>
    /// Displays a tooltip if the last item was hovered over.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="flags">Item hover flags.</param>
    public static void TextOnHover(string text, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
    {
        if (!ImGui.IsItemHovered((int)flags))
            return;

        ImGui.BeginTooltip();
        ImGui.Text(text);
        ImGui.EndTooltip();
    }
}
