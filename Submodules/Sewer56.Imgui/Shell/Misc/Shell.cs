using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Utilities;

// ReSharper disable once CheckNamespace
namespace Sewer56.Imgui.Shell;

public static partial class Shell
{
    /// <summary>
    /// Provides access to the current ImGui style.
    /// </summary>
    public static ImGuiStyle Style;

    /// <summary>
    /// Provides access to the current ImGui IO.
    /// </summary>
    public static ImGuiIO IO;

    /// <summary>
    /// Provides access to the current ImGui IO.
    /// </summary>
    public static ImFont MonoFont;

    public static unsafe void SetupImGuiConfig(string modFolder)
    {
        IO = ImGui.GetIO();
        IO.BackendFlags |= (int)ImGuiBackendFlags.ImGuiBackendFlagsHasSetMousePos;
        IO.ConfigFlags |= (int)ImGuiConfigFlags.ImGuiConfigFlagsNavEnableKeyboard;
        IO.ConfigFlags &= ~(int)ImGuiConfigFlags.ImGuiConfigFlagsNavEnableSetMousePos;
        ((ImGuiIO.__Internal*)IO.__Instance)->IniFilename = Marshal.StringToHGlobalAnsi("tweakbox.imgui.ini");

        var monoFontPath = Path.Combine(modFolder, "Assets/Fonts/RobotoMono-Bold.ttf");
        MonoFont = ImGui.ImFontAtlasAddFontFromFileTTF(IO.Fonts, monoFontPath, 15.0f, null, ref Constants.NullReference<ushort>());

        var fontPath = Path.Combine(modFolder, "Assets/Fonts/Ruda-Bold.ttf");
        using var font = ImGui.ImFontAtlasAddFontFromFileTTF(IO.Fonts, fontPath, 15.0f, null, ref Constants.NullReference<ushort>());
        if (font != null)
            IO.FontDefault = font;

        Style = ImGui.GetStyle();
        Style.FrameRounding = 4.0f;
        Style.WindowRounding = 4.0f;
        Style.WindowBorderSize = 0.0f;
        Style.PopupBorderSize = 0.0f;
        Style.GrabRounding = 4.0f;

        using var colors = new FinalizedList<ImVec4[], ImVec4>(Style.Colors);
        colors.Instance[(int)ImGuiCol.ImGuiColText] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTextDisabled] = new Vector4(0.73f, 0.75f, 0.74f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColWindowBg] = new Vector4(0.09f, 0.09f, 0.09f, 0.94f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColPopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColBorder] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColBorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColFrameBg] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColFrameBgHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.40f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColFrameBgActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTitleBg] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTitleBgActive] = new Vector4(0.47f, 0.22f, 0.22f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTitleBgCollapsed] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColMenuBarBg] = new Vector4(0.34f, 0.16f, 0.16f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColCheckMark] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColSliderGrab] = new Vector4(0.71f, 0.39f, 0.39f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColSliderGrabActive] = new Vector4(0.84f, 0.66f, 0.66f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColButton] = new Vector4(0.47f, 0.22f, 0.22f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColButtonHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColButtonActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColHeader] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColHeaderHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColHeaderActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColSeparator] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColSeparatorHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColSeparatorActive] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColResizeGrip] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColResizeGripHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColResizeGripActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTab] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTabHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTabActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTabUnfocused] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTabUnfocusedActive] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColPlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColPlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColPlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColPlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColTextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColDragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColNavHighlight] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColNavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColNavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f).ToImVec();
        colors.Instance[(int)ImGuiCol.ImGuiColModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f).ToImVec();
        Style.Colors = colors;
    }
}
