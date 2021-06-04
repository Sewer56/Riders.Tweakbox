using System;
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
        public float CurrentWidth { get; private set; } = 0;
        public float WidthAvailable { get; private set; }
        public int ItemWidth { get; private set; }
        public float ItemOffset { get; private set; }

        /// <summary>
        /// Initializes the content wrapper.
        /// </summary>
        public unsafe ContentWrapper(int itemWidth, float availableWidth = -1, float itemItemOffset = 20)
        {
            WidthAvailable = availableWidth != -1 
                ? availableWidth 
                : ImGui.GetWindowContentRegionWidth();

            ItemOffset = itemItemOffset;
            ItemWidth   = itemWidth;
            CurrentWidth = 0;
        }

        /// <summary>
        /// Execute this function after placing an item.
        /// </summary>
        /// <param name="lastItem">Set true if the item is the last item to be placed.</param>
        public unsafe void AfterPlaceItem(bool lastItem)
        {
            Vector2 lastItemSize;
            ImGui.__Internal.GetItemRectSize((IntPtr) (&lastItemSize));

            // Set current/next line.
            var offset = ItemWidth - lastItemSize.X;
            CurrentWidth += ItemWidth;
            if (CurrentWidth + ItemWidth < WidthAvailable && !lastItem)
            {
                ImGui.SameLine(0, offset + ItemOffset);
                CurrentWidth += (ItemOffset);
            }
            else
                CurrentWidth = 0;
        }
    }
}
