using System;
using System.Collections.Generic;
using DearImguiSharp;

namespace Riders.Tweakbox.Definitions
{
    // TODO: Add notification system (messages show bottom right of screen).
    public static class Shell
    {
        /// <summary>
        /// All dialogs available for the shell.
        /// </summary>
        private static List<Func<bool>> _widgets = new List<Func<bool>>();

        /// <summary>
        /// Adds a dialog to the current shell.
        /// </summary>
        /// <param name="dialog">The dialog to run, returns false to remove the dialog, else true to keep displaying it.</param>
        public static void AddCustom(Func<bool> dialog)
        {
            lock (_widgets)
            {
                _widgets.Add(dialog);
            }
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static void AddDialog(string name, DialogFn dialogFunction)
        {
            AddCustom(() => DialogHandler(name, dialogFunction));
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static void AddWindow(string name, DialogFn dialogFunction, ImGuiWindowFlags flags = 0)
        {
            AddCustom(() => WindowHandler(name, dialogFunction, flags));
        }

        /// <summary>
        /// Renders the current dialogs to the screen.
        /// </summary>
        internal static void Render()
        {
            lock (_widgets)
            {
                for (int x = _widgets.Count - 1; x >= 0; x--)
                {
                    if (!_widgets[x]())
                        _widgets.RemoveAt(x);
                }
            }
        }

        /// <summary>
        /// Wraps a dialog.
        /// </summary>
        private static bool DialogHandler(string name, DialogFn sup)
        {
            bool isOpened = true;
            ImGui.OpenPopup(name);
            if (ImGui.BeginPopupModal(name, ref isOpened, (int)ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
                sup(ref isOpened);

            ImGui.EndPopup();
            ImGui.CloseCurrentPopup();
            return isOpened;
        }

        /// <summary>
        /// Wraps a window.
        /// </summary>
        private static bool WindowHandler(string name, DialogFn sup, ImGuiWindowFlags flags = 0)
        {
            bool isOpened = true;
            if (ImGui.Begin(name, ref isOpened, (int) flags)) 
                sup(ref isOpened);

            ImGui.End();
            return isOpened;
        }

        #region Delegates
        public delegate void DialogFn(ref bool isOpened);
        #endregion
    }
}
