using System;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using static Riders.Tweakbox.Misc.Native;
using Task = System.Threading.Tasks.Task;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Riders.Tweakbox.Components.Common;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditorConfig : JsonConfigBase<TweaksEditorConfig, TweaksEditorConfig.Internal>
    {
        // Apply
        public override unsafe void Apply()
        {
            *Sewer56.SonicRiders.API.Misc.Blur = Data.Blur;

            //ResetDevice();
            ChangeBorderless(Data.Borderless);
            ConfigUpdated?.Invoke();
        }

        public unsafe void ApplyStartup()
        {
            *Sewer56.SonicRiders.API.Misc.ResolutionX = Data.ResolutionX;
            *Sewer56.SonicRiders.API.Misc.ResolutionY = Data.ResolutionY;

            if (Data.Fullscreen == true)
                *Sewer56.SonicRiders.API.Misc.MultiSampleType = 0;

            *Sewer56.SonicRiders.API.Misc.Fullscreen = Data.Fullscreen;
            Apply();
        }

        public unsafe void ChangeBorderless(bool borderless)
        {
            // Reset Game Window
            var handle = Sewer56.SonicRiders.API.Window.WindowHandle;
            if (handle != IntPtr.Zero)
            {
                const int GWL_STYLE = -16;
                var style = PInvoke.GetWindowLong(new HWND(handle), GWL_STYLE);

                if (style == 0) 
                    return;

                var flags = (WindowStyles) style;
                if (borderless) 
                    RemoveBorder(ref flags);
                else
                    AddBorder(ref flags);
                
                PInvoke.SetWindowLong(new HWND(handle), GWL_STYLE, (int) flags);
                Task.Delay(100).ContinueWith((x) => ResizeWindow(*Sewer56.SonicRiders.API.Misc.ResolutionX, *Sewer56.SonicRiders.API.Misc.ResolutionY, handle));
            }
        }

        public void RemoveBorder(ref WindowStyles flags)
        {
            flags &= ~WindowStyles.WS_CAPTION;
            flags &= ~WindowStyles.WS_MINIMIZEBOX;
        }

        public void AddBorder(ref WindowStyles flags)
        {
            flags |= WindowStyles.WS_CAPTION;
            flags |= WindowStyles.WS_MINIMIZEBOX;
        }

        public unsafe void ResetDevice()
        {
            // Currently Unused
            // Reset Game Window
            var handle = Sewer56.SonicRiders.API.Window.WindowHandle;
            if (handle != IntPtr.Zero)
            {
                // Resize Window
                ResizeWindow(Data.ResolutionX, Data.ResolutionY, handle);
                
                // Reset D3D Device
                // TODO: Write code to recreate all textures and possibly other assets, as described in Reset() function.
                var controller = IoC.Get<GraphicsController>();
                var presentParametersCopy = controller.LastPresentParameters;
                presentParametersCopy.BackBufferHeight = Data.ResolutionY;
                presentParametersCopy.BackBufferWidth  = Data.ResolutionX;
                controller.D3dDeviceEx.ResetEx(ref presentParametersCopy);
            }
        }

        private void GetWindowSizeWithBorder(IntPtr handle, int x, int y, out int newX, out int newY)
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

        private void ResizeWindow(int x, int y, IntPtr handle, bool centered = true)
        {
            GetWindowSizeWithBorder(handle, x, y, out var newX, out var newY);

            int left = 0;
            int top  = 0;

            if (centered)
            {
                var monitor = PInvoke.MonitorFromWindow(new HWND(handle), MONITOR_DEFAULTTONEAREST);
                var info = new MONITORINFO { cbSize = (uint)Struct.GetSize<MONITORINFO>() };

                if (PInvoke.GetMonitorInfo(monitor, ref info))
                {
                    left += (info.rcMonitor.right - newX) / 2;
                    top += (info.rcMonitor.bottom - newY) / 2;
                }
            }
            
            PInvoke.MoveWindow(new HWND(handle), left, top, newX, newY, true);
        }

        #region Internal
        public class Internal
        {
            public bool BootToMenu = true;
            public bool FramePacing = true;
            public bool FramePacingSpeedup = true; // Speed up game to compensate for lag.
            public float DisableYieldThreshold = 80;
            public bool D3DDeviceFlags = true;
            public bool DisableVSync = true;
            public bool AutoQTE = true;
            public int ResolutionX = 1280;
            public int ResolutionY = 720;
            public bool Fullscreen = false;
            public bool Blur = false;
            public bool WidescreenHack = false;
            public bool Borderless = false;
            public bool SinglePlayerStageData = true;
            public bool SinglePlayerModels = true;
        }
        #endregion
    }
}
