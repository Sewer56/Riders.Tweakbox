﻿using System;
using System.Numerics;
using DearImguiSharp;

namespace Sewer56.Imgui.Layout
{
    /// <summary>
    /// A utility class that helps you wrap collections of items as a grid by manually performing some simple math.
    /// To use this class, simply create this class, then call <see cref="AfterPlaceItem"/> after adding each individual item.
    /// </summary>
    public class ContentWrapper
    {
        public int? InitialX { get; private set; }
        public int CurrentX { get; private set; } = 0;
        public Vector2 BottomRight { get; private set; }
        public int ItemWidth { get; private set; }

        /// <summary>
        /// Initializes the content wrapper.
        /// </summary>
        public unsafe ContentWrapper(int itemWidth)
        {
            var vec2 = new Vector2();
            ImGui.__Internal.GetContentRegionAvail((IntPtr) (&vec2));

            BottomRight = vec2;
            ItemWidth   = itemWidth;
        }

        /// <summary>
        /// Execute this function after placing an item.
        /// </summary>
        /// <param name="lastItem">Set true if the item is the last item to be placed.</param>
        public void AfterPlaceItem(bool lastItem)
        {
            SetInitialX();
            
            // Set current/next line.
            CurrentX += ItemWidth;
            if (CurrentX + ItemWidth < BottomRight.X && !lastItem)
                ImGui.SameLine(CurrentX, 0);
            else
                CurrentX = InitialX.Value;
        }

        /// <summary>
        /// Sets <see cref="InitialX"/> if it is not set.
        /// </summary>
        private unsafe void SetInitialX()
        {
            if (!InitialX.HasValue)
            {
                var windowPos = new Vector2();
                var rectMin = new Vector2();

                ImGui.__Internal.GetWindowPos((IntPtr)(&windowPos));
                ImGui.__Internal.GetItemRectMin((IntPtr)(&rectMin));

                InitialX = (int)(rectMin.X - windowPos.X);
                CurrentX = InitialX.Value;
            }
        }
    }
}
