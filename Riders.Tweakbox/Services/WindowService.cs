using System;
using System.Threading.Tasks;
using Reloaded.Memory;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
namespace Riders.Tweakbox.Services;

/// <summary>
/// A service that allows you to interact with the game window.
/// </summary>
public class WindowService : ISingletonService
{
    public WindowService() { }

    /// <summary>
    /// Sets the borderless state for a given window.
    /// </summary>
    public unsafe void SetBorderless(bool borderless, IntPtr handle)
    {
        var style = PInvoke.GetWindowLong(new HWND(handle), WINDOW_LONG_PTR_INDEX.GWL_STYLE);

        if (style == 0)
            return;

        var flags = (Native.WindowStyles)style;
        ToggleBorder(borderless, ref flags);

        PInvoke.SetWindowLong(new HWND(handle), WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)flags);
        Task.Delay(100).ContinueWith((x) => ResizeWindow(*Sewer56.SonicRiders.API.Misc.ResolutionX, *Sewer56.SonicRiders.API.Misc.ResolutionY, handle));
    }

    /// <summary>
    /// Resizes the game window.
    /// </summary>
    /// <param name="x">Width</param>
    /// <param name="y">Height</param>
    /// <param name="handle">Game's Window Handle</param>
    /// <param name="centered">Whether the window should be centered to screen.</param>
    public void ResizeWindow(int x, int y, IntPtr handle, bool centered = true)
    {
        var rect = new RECT()
        {
            left = 0,
            top = 0,
            bottom = y,
            right = x
        };

        var style = PInvoke.GetWindowLong((HWND)handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        var adjust = PInvoke.AdjustWindowRect(ref rect, (WINDOW_STYLE)style, false);

        int left = 0;
        int top = 0;
        int width = rect.right - rect.left;
        int height = rect.bottom - rect.top;

        if (centered)
        {
            var monitor = PInvoke.MonitorFromWindow(new HWND(handle), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            var info = new MONITORINFO { cbSize = (uint)Struct.GetSize<MONITORINFO>() };

            if (PInvoke.GetMonitorInfo(monitor, ref info))
            {
                left += (info.rcMonitor.right - width) / 2;
                top += (info.rcMonitor.bottom - height) / 2;
            }
        }

        PInvoke.MoveWindow(new HWND(handle), left, top, width, height, true);
    }

    public void ToggleBorder(bool borderless, ref Native.WindowStyles flags)
    {
        if (borderless)
            RemoveBorder(ref flags);
        else
            AddBorder(ref flags);
    }

    public void RemoveBorder(ref Native.WindowStyles flags)
    {
        flags &= ~Native.WindowStyles.WS_CAPTION;
        flags &= ~Native.WindowStyles.WS_MINIMIZEBOX;
    }

    public void AddBorder(ref Native.WindowStyles flags)
    {
        flags |= Native.WindowStyles.WS_CAPTION;
        flags |= Native.WindowStyles.WS_MINIMIZEBOX;
    }
}
