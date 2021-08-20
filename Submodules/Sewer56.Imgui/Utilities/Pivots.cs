using System;
using System.Numerics;
using DearImguiSharp;
namespace Sewer56.Imgui.Utilities;

public static class Pivots
{
    /// <summary>
    /// Translates a given pivot to a relative point (0.0,0.0) to (1.0,1.0).
    /// </summary>
    public static Vector2 GetPoint(Pivot pivot)
    {
        switch (pivot)
        {
            case Pivot.TopLeft:
                return new Vector2(0, 0);
            case Pivot.Top:
                return new Vector2(0.5f, 0);
            case Pivot.TopRight:
                return new Vector2(1.0f, 0);
            case Pivot.Left:
                return new Vector2(0, 0.5f);
            case Pivot.Center:
                return new Vector2(0.5f, 0.5f);
            case Pivot.Right:
                return new Vector2(1.0f, 0.5f);
            case Pivot.BottomLeft:
                return new Vector2(0.0f, 1.0f);
            case Pivot.Bottom:
                return new Vector2(0.5f, 1.0f);
            case Pivot.BottomRight:
                return new Vector2(1.0f, 1.0f);
            default:
                throw new ArgumentOutOfRangeException(nameof(pivot), pivot, null);
        }
    }

    public enum Pivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }
}
