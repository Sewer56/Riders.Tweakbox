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
            const int GWL_STYLE = -16;
            var rect = new RECT()
            {
                left = 0,
                top = 0,
                bottom = y,
                right = x
            };

            var style  = PInvoke.GetWindowLong((HWND) handle, GWL_STYLE);
            var adjust = PInvoke.AdjustWindowRect(ref rect, (uint) style, false);

            int left = 0;
            int top  = 0;
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            if (centered)
            {
                var monitor = PInvoke.MonitorFromWindow(new HWND(handle), Native.MONITOR_DEFAULTTONEAREST);
                var info = new MONITORINFO { cbSize = (uint)Struct.GetSize<MONITORINFO>() };

                if (PInvoke.GetMonitorInfo(monitor, ref info))
                {
                    left += (info.rcMonitor.right - width) / 2;
                    top += (info.rcMonitor.bottom - height) / 2;
                }
            }
            
            PInvoke.MoveWindow(new HWND(handle), left, top, width, height, true);
        }
    }
}
