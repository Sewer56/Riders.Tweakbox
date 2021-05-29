﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DearImguiSharp;
using Sewer56.Imgui.Shell.Structures;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiWindowFlags;

namespace Sewer56.Imgui.Shell
{
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
        public static void AddDialog(string name, DialogFn dialogFunction, Action onClose = null,
            ImGuiWindowFlags flags = ImGuiWindowFlagsAlwaysAutoResize, bool showClose = true)
        {
            AddCustom(() => DialogHandler(name, dialogFunction, onClose, flags, showClose));
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static async Task AddDialogAsync(string name, DialogFn dialogFunction, Action onClose = null,
            ImGuiWindowFlags flags = ImGuiWindowFlagsAlwaysAutoResize, bool showClose = true)
        {
            bool hasFinished = false;

            AddCustom(() => DialogHandler(name, dialogFunction, () =>
            {
                onClose?.Invoke();
                hasFinished = true;
            }, flags, showClose));

            while (!hasFinished)
                await Task.Delay(16);
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static void AddDialog(string name, string dialogText, Action onClose = null,
            ImGuiWindowFlags flags = ImGuiWindowFlagsAlwaysAutoResize, bool showClose = true)
        {
            AddCustom(() => DialogHandler(name, (ref bool opened) => ImGui.Text(dialogText), onClose, flags, showClose));
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static async Task AddDialogAsync(string name, string dialogText, Action onClose = null,
            ImGuiWindowFlags flags = ImGuiWindowFlagsAlwaysAutoResize, bool showClose = true)
        {
            bool hasFinished = false;
            
            AddCustom(() => DialogHandler(name, (ref bool opened) => ImGui.Text(dialogText), () =>
            {
                onClose?.Invoke();
                hasFinished = true;
            }, flags, showClose));

            while (!hasFinished)
                await Task.Delay(16);
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static void AddWindow(string name, DialogFn dialogFunction, Action onClose = null, ImGuiWindowFlags flags = 0)
        {
            AddCustom(() => WindowHandler(name, dialogFunction, onClose, flags));
        }

        /// <summary>
        /// Adds a function that displays a dialog to the current shell.
        /// </summary>
        public static async Task AddWindowAsync(string name, DialogFn dialogFunction, Action onClose = null, ImGuiWindowFlags flags = 0)
        {
            bool hasFinished = false;

            AddCustom(() => WindowHandler(name, dialogFunction, () =>
            {
                onClose?.Invoke();
                hasFinished = true;
            }, flags));

            while (!hasFinished)
                await Task.Delay(16);
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
        public static void Log(in LogItem item)
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
        private static unsafe bool DialogHandler(string name, DialogFn sup, Action onClose = null, ImGuiWindowFlags flags = ImGuiWindowFlagsAlwaysAutoResize, bool showClose = true)
        {
            bool isOpened = true;
            ImGui.OpenPopup(name, (int) ImGuiPopupFlags.ImGuiPopupFlagsNoOpenOverExistingPopup);

            bool beginSuccess = showClose ? 
                ImGui.BeginPopupModal(name, ref isOpened, (int) flags) : 
                ImGui.__Internal.BeginPopupModal(name, null, (int) flags);

            if (beginSuccess)
            {
                sup(ref isOpened);
                ImGui.EndPopup();
            }

            if (!isOpened)
                onClose?.Invoke();

            return isOpened;
        }

        /// <summary>
        /// Wraps a window.
        /// </summary>
        private static bool WindowHandler(string name, DialogFn sup, Action onClose = null, ImGuiWindowFlags flags = 0)
        {
            bool isOpened = true;
            if (ImGui.Begin(name, ref isOpened, (int) flags))
            {
                sup(ref isOpened);
                ImGui.End();
            }

            if (!isOpened)
                onClose?.Invoke();

            return isOpened;
        }

        #region Delegates
        public delegate void DialogFn(ref bool isOpened);
        #endregion
    }
}
