using System;
using System.Collections.Generic;
using System.Text;
using DearImguiSharp;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Definitions
{
    /// <summary>
    /// Renders an individual menu bar.
    /// </summary>
    public class MenuBar
    {
        /// <summary>
        /// All menus to be rendered.
        /// </summary>
        public IList<MenuBarItem> Menus { get; set; }
        
        /// <summary>
        /// All pieces of text to be rendered.
        /// </summary>
        public List<string> Text { get; set; }

        /// <summary>
        /// Renders the content of the Menu Bar.
        /// </summary>
        public void Render()
        {
            ImGui.BeginMainMenuBar();

            // Get size of main menu.
            var menuSize = Utilities.RunVectorFunction(ImGui.GetWindowSize);

            // Render all menus.
            foreach (var menu in Menus)
                menu.Render(ref menu.IsEnabled());

            // Render text.
            var vector        = new ImVec2();
            int currentOffset = (int) Constants.Spacing;
            foreach (var text in Text)
            {
                ImGui.CalcTextSize(vector, text, null, false, -1.0f);
                currentOffset += (int) vector.X;
                currentOffset += (int) Constants.Spacing;
                ImGui.SameLine(menuSize.X - currentOffset, 0);
                ImGui.Text(text);
            }

            ImGui.EndMainMenuBar();
        }

        /// <summary>
        /// Suspends all menu activity.
        /// </summary>
        public void Suspend()
        {
            foreach (var menu in Menus)
                menu.Disable();
        }

        /// <summary>
        /// Resumes all menu activity.
        /// </summary>
        public void Resume()
        {
            foreach (var menu in Menus)
                menu.Enable();
        }
    }
}
