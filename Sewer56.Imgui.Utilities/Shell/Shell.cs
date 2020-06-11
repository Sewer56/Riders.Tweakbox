using System;
using System.Collections.Generic;
using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell.Structures;
using Sewer56.Imgui.Utilities;

namespace Sewer56.Imgui.Shell
{
    // TODO: Add notification system (messages show bottom right of screen).
    public static partial class Shell
    {
        /// <summary>
        /// If true, renders the log, else false.
        /// </summary>
        public static bool EnableLog { get; set; } = true;

        /// <summary>
        /// All dialogs available for the shell.
        /// </summary>
        private static List<Func<bool>> _widgets = new List<Func<bool>>();

        /// <summary>
        /// Keeps all log text available.
        /// </summary>
        private static Queue<LogItem> _logs = new Queue<LogItem>();

        /// <summary>
        /// The log window.
        /// </summary>
        private static InformationWindow _logWindow = new InformationWindow("Sewer56.Imgui Log", Pivots.Pivot.BottomLeft, Pivots.Pivot.BottomLeft);

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
        public static void AddDialog(string name, string dialogText)
        {
            AddCustom(() => DialogHandler(name, (ref bool opened) => ImGui.RenderTextWrapped(Constants.DefaultVector2, dialogText, null, -1.0f)));
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
        public static void Render()
        {
            RenderWidgets();
            RenderLog();
        }

        /// <summary>
        /// Adds a new item onto the log window.
        /// </summary>
        /// <param name="item">The item to log.</param>
        public static void Log(LogItem item)
        {
            lock (_logs) 
                _logs.Enqueue(item);
        }

        private static void RenderWidgets()
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

        private static void RenderLog()
        {
            if (!EnableLog)
                return;

            lock (_logs)
            {
                var totalItems = _logs.Count;
                if (totalItems <= 0) 
                    return;

                _logWindow.Begin();
                int itemsRendered = 0;
                while (itemsRendered < totalItems && _logs.TryDequeue(out var logItem))
                {
                    // Render Item
                    ImGui.Text(logItem.Text);

                    // Re-queue item if necessary.
                    if (!logItem.HasExpired()) 
                        _logs.Enqueue(logItem);

                    itemsRendered += 1;
                }

                _logWindow.End();
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
