using System;
using System.Numerics;
using DearImguiSharp;

namespace Sewer56.Imgui.Layout
{
    /// <summary>
    /// A class that helps with centering a single dear imgui element on a single line.
    /// </summary>
    public class HorizontalCenterHelper
    {
        /// <summary>
        /// Width of the item after the last call to <see cref="End"/>
        /// </summary>
        public float LastItemWidth;

        /// <summary>
        /// Call this before placing your control.
        /// </summary>
        public void Begin(float? availableWidth = null)
        {
            ImGui.NewLine();
            availableWidth ??= ImGui.GetWindowContentRegionWidth();
            ImGui.SameLine((availableWidth.Value - LastItemWidth) / 2, 0);
        }

        /// <summary>
        /// Call this after placing your control.
        /// </summary>
        public unsafe void End()
        {
            Vector2 size;
            ImGui.__Internal.GetItemRectSize((IntPtr) (&size));
            LastItemWidth = size.X;
        }
    }
}
