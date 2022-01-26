using System;
using System.Runtime.CompilerServices;
using DearImguiSharp;
using Sewer56.SonicRiders.Structures.Misc;

namespace Riders.Tweakbox.Misc;

public static class ImGuiExtensions
{
    public static unsafe void RenderIntAsBytesHex(void* address, int spacing, ref int id, int width = 80)
    {
        ImGui.PushItemWidth(width);
        ImGui.BeginGroup();

        ImGui.PushID_Int(id++);
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr)((byte*)address), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((byte*)address + 0)):X2}", (int)1);
        ImGui.PopID();

        ImGui.SameLine(0, spacing);

        ImGui.PushID_Int(id++); 
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr)((byte*)address + 1), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((byte*)address + 1)):X2}", (int)1);
        ImGui.PopID();

        ImGui.SameLine(0, spacing);

        ImGui.PushID_Int(id++); 
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr)((byte*)address + 2), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((byte*)address + 2)):X2}", (int)1);
        ImGui.PopID();

        ImGui.SameLine(0, spacing);

        ImGui.PushID_Int(id++); 
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr)((byte*)address + 3), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((byte*)address + 3)):X2}", (int)1);
        ImGui.PopID();

        ImGui.EndGroup();
        ImGui.PopItemWidth();
    }

    public static unsafe void RenderIntAsShortsHex(void* address, int spacing, ref int id, int width = 80)
    {
        ImGui.PushItemWidth(width);
        ImGui.BeginGroup();
        ImGui.PushID_Int(id++);
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr)(address), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((ushort*)address)):X4}", (int)1);
        ImGui.PopID();

        ImGui.SameLine(0, spacing);

        ImGui.PushID_Int(id++);
        ImGui.DragScalar("", (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr)((ushort*)address + 1), 0.1f, IntPtr.Zero, IntPtr.Zero, $"{(*((ushort*)address + 1)):X4}", (int)1);
        ImGui.PopID();
        ImGui.EndGroup();
        ImGui.PopItemWidth();
    }

    public static unsafe void RenderColourPickerForRgba(ref ColorRGBA color, string label)
    {
        Span<float> values = stackalloc float[4];
        ColourRgbaToFloat(color, values);
        ImGui.Custom.ColorEdit4(label, (float*)Unsafe.AsPointer(ref values.GetPinnableReference()), 1);
        color = FloatRgbaToColour(values);
    }

    public static unsafe void RenderColourPickerForAbgr(ref ColorABGR color, string label)
    {
        Span<float> values = stackalloc float[4];
        ColourAbgrToFloat(color, values);
        ImGui.Custom.ColorEdit4(label, (float*)Unsafe.AsPointer(ref values.GetPinnableReference()), 1);
        color = FloatArgbToColour(values);
    }

    public static void ColourRgbaToFloat(ColorRGBA color, Span<float> colors)
    {
        colors[0] = color.Red / 255.0f;
        colors[1] = color.Green / 255.0f;
        colors[2] = color.Blue / 255.0f;
        colors[3] = color.Alpha / 255.0f;
    }

    public static ColorRGBA FloatRgbaToColour(Span<float> colors)
    {
        return new ColorRGBA()
        {
            Red = (byte)(colors[0] * 255.0f),
            Green = (byte)(colors[1] * 255.0f),
            Blue = (byte)(colors[2] * 255.0f),
            Alpha = (byte)(colors[3] * 255.0f)
        };
    }

    public static void ColourAbgrToFloat(ColorABGR color, Span<float> colors)
    {
        colors[0] = color.Red / 255.0f;
        colors[1] = color.Green / 255.0f;
        colors[2] = color.Blue / 255.0f;
        colors[3] = color.Alpha / 255.0f;
    }

    public static ColorABGR FloatArgbToColour(Span<float> colors)
    {
        return new ColorABGR()
        {
            Red = (byte)(colors[0] * 255.0f),
            Green = (byte)(colors[1] * 255.0f),
            Blue = (byte)(colors[2] * 255.0f),
            Alpha = (byte)(colors[3] * 255.0f)
        };
    }
}