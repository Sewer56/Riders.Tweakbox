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
        IO.BackendFlags |= (int)ImGuiBackendFlags.HasSetMousePos;
        IO.ConfigFlags |= (int)ImGuiConfigFlags.NavEnableKeyboard;
        IO.ConfigFlags &= ~(int)ImGuiConfigFlags.NavEnableSetMousePos;
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
        colors.Instance[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.TextDisabled] = new Vector4(0.73f, 0.75f, 0.74f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.09f, 0.94f).ToImVec();
        colors.Instance[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f).ToImVec();
        colors.Instance[(int)ImGuiCol.Border] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.FrameBg] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.40f).ToImVec();
        colors.Instance[(int)ImGuiCol.FrameBgActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.TitleBg] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.TitleBgActive] = new Vector4(0.47f, 0.22f, 0.22f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
        colors.Instance[(int)ImGuiCol.MenuBarBg] = new Vector4(0.34f, 0.16f, 0.16f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f).ToImVec();
        colors.Instance[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.CheckMark] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.SliderGrab] = new Vector4(0.71f, 0.39f, 0.39f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.84f, 0.66f, 0.66f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.Button] = new Vector4(0.47f, 0.22f, 0.22f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.ButtonHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.Header] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.HeaderHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.65f).ToImVec();
        colors.Instance[(int)ImGuiCol.HeaderActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.Separator] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f).ToImVec();
        colors.Instance[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.SeparatorActive] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ResizeGrip] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.Tab] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
        colors.Instance[(int)ImGuiCol.TabHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.TabActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
        colors.Instance[(int)ImGuiCol.TabUnfocused] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f).ToImVec();
        colors.Instance[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f).ToImVec();
        colors.Instance[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f).ToImVec();
        colors.Instance[(int)ImGuiCol.NavHighlight] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
        colors.Instance[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f).ToImVec();
        colors.Instance[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f).ToImVec();
        colors.Instance[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f).ToImVec();
        Style.Colors = colors;
    }
}
