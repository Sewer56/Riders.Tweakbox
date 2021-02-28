using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
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
        public void Begin()
        {
            ImGui.NewLine();
            var availableWidth = ImGui.GetWindowContentRegionWidth();
            ImGui.SameLine(0, (availableWidth - LastItemWidth) / 2);
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
