using System;
using Reloaded.Memory;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;

namespace Riders.Tweakbox.Services
{
    /// <summary>
    /// A service that allows you to interact with the game window.
    /// </summary>
    public class WindowService : ISingletonService
    {
        public WindowService() { }

        /// <summary>
        /// Resizes the game window.
        /// </summary>
        /// <param name="x">Width</param>
        /// <param name="y">Height</param>
        /// <param name="handle">Game's Window Handle</param>
        /// <param name="centered">Whether the window should be centered to screen.</param>
        public void ResizeWindow(int x, int y, IntPtr handle, bool centered = true)
        {
            GetWindowSizeWithBorder(handle, x, y, out var newX, out var newY);

            int left = 0;
            int top  = 0;

            if (centered)
            {
                var monitor = PInvoke.MonitorFromWindow(new HWND(handle), Native.MONITOR_DEFAULTTONEAREST);
                var info = new MONITORINFO { cbSize = (uint)Struct.GetSize<MONITORINFO>() };

                if (PInvoke.GetMonitorInfo(monitor, ref info))
                {
                    left += (info.rcMonitor.right - newX) / 2;
                    top += (info.rcMonitor.bottom - newY) / 2;
                }
            }
            
            PInvoke.MoveWindow(new HWND(handle), left, top, newX, newY, true);
        }

        public void GetWindowSizeWithBorder(IntPtr handle, int x, int y, out int newX, out int newY)
        {
            // get size of window and the client area
            PInvoke.GetWindowRect(new HWND(handle), out var windowRect);
            PInvoke.GetClientRect(new HWND(handle), out var clientRect);

            // calculate size of non-client area
            int extraX = windowRect.right - windowRect.left - clientRect.right;
            int extraY = windowRect.bottom - windowRect.top - clientRect.bottom;
            newX = x + extraX;
            newY = y + extraY;
        }
    }
}
