using System.Numerics;
using DearImguiSharp;
using Sewer56.Imgui.Controls.Extensions;
using static DearImguiSharp.ImGuiItemFlags;
using static DearImguiSharp.ImGuiStyleVar;
namespace Sewer56.Imgui.Utilities;

public static class Utilities
{
    /// <summary>
    /// Disables all controls rendered after this call.
    /// </summary>
    public static void PushDisabled()
    {
        ImGui.PushItemFlag((int)ImGuiItemFlagsDisabled, true);
        ImGui.PushStyleVarFloat((int)ImGuiStyleVarAlpha, Shell.Shell.Style.Alpha * 0.5f);
    }

    /// <summary>
    /// Re-enables all controls rendered after this call.
    /// </summary>
    public static void PopDisabled()
    {
        ImGui.PopItemFlag();
        ImGui.PopStyleVar(1);
    }

    /// <summary/>
    public static FinalizedImVec4 ToImVec(this Vector4 vector)
    {
        return ToImVec(vector, new FinalizedImVec4());
    }

    /// <summary/>
    public static FinalizedImVec4 ToImVec(this Vector4 vector, FinalizedImVec4 imVec4)
    {
        imVec4.X = vector.X;
        imVec4.Y = vector.Y;
        imVec4.Z = vector.Z;
        imVec4.W = vector.W;
        return imVec4;
    }
    
    /// <summary/>
    public static Vector4 ToVector(this ImVec4 vector) => new Vector4(vector.X, vector.Y, vector.Z, vector.W);

    /// <summary/>
    public static ImVec2 ToImVec(this Vector2 vector, ImVec2 vec2)
    {
        vec2.X = vector.X;
        vec2.Y = vector.Y;
        return vec2;
    }

    /// <summary/>
    public static FinalizedImVec2 ToImVec(this Vector2 vector, FinalizedImVec2 vec2) => (FinalizedImVec2) ToImVec(vector, (ImVec2)vec2);

    /// <summary/>
    public static ImVec2 ToImVec(this Vector2 vector) => ToImVec(vector, new ImVec2());

    /// <summary>
    /// Converts a hex RGBA colour into a <see cref="ImVec4"/>.
    /// </summary>
    public static ImVec4 HexToFloat(uint hex)
    {
        return new ImVec4()
        {
            X = ((hex >> 24) & 0xFF) / (float)byte.MaxValue * 1.00f,
            Y = ((hex >> 16) & 0xFF) / (float)byte.MaxValue * 1.00f,
            Z = ((hex >> 8) & 0xFF) / (float)byte.MaxValue * 1.00f,
            W = (hex & 0xFF) / (float)byte.MaxValue * 1.00f,
        };
    }

    /// <summary>
    /// Converts a hex RGBA colour into a <see cref="ImVec4.__Internal"/>.
    /// </summary>
    public static ImVec4.__Internal HexToFloatInternal(uint hex)
    {
        return new ImVec4.__Internal()
        {
            x = ((hex >> 24) & 0xFF) / (float)byte.MaxValue * 1.00f,
            y = ((hex >> 16) & 0xFF) / (float)byte.MaxValue * 1.00f,
            z = ((hex >> 8) & 0xFF) / (float)byte.MaxValue * 1.00f,
            w = (hex & 0xFF) / (float)byte.MaxValue * 1.00f,
        };
    }
}
