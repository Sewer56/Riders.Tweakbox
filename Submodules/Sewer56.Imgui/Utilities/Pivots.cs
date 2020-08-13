using System;
using DearImguiSharp;

namespace Sewer56.Imgui.Utilities
{
    public static class Pivots
    {
        /// <summary>
        /// Translates a given pivot to a relative point (0.0,0.0) to (1.0,1.0).
        /// </summary>
        public static ImVec2 GetPoint(Pivot pivot)
        {
            switch (pivot)
            {
                case Pivot.TopLeft:
                    return new System.Numerics.Vector2(0, 0).ToImVec();
                case Pivot.Top:
                    return new System.Numerics.Vector2(0.5f, 0).ToImVec();
                case Pivot.TopRight:
                    return new System.Numerics.Vector2(1.0f, 0).ToImVec();
                case Pivot.Left:
                    return new System.Numerics.Vector2(0, 0.5f).ToImVec();
                case Pivot.Center:
                    return new System.Numerics.Vector2(0.5f, 0.5f).ToImVec();
                case Pivot.Right:
                    return new System.Numerics.Vector2(1.0f, 0.5f).ToImVec();
                case Pivot.BottomLeft:
                    return new System.Numerics.Vector2(0.0f, 1.0f).ToImVec();
                case Pivot.Bottom:
                    return new System.Numerics.Vector2(0.5f, 1.0f).ToImVec();
                case Pivot.BottomRight:
                    return new System.Numerics.Vector2(1.0f, 1.0f).ToImVec();
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
}
