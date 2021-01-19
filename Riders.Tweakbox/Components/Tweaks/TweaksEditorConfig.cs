using System;
using MessagePack;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32_Gdi;
using Task = System.Threading.Tasks.Task;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditorConfig : IConfiguration
    {
        public Internal Data = Internal.GetDefault();
        
        // Serialization

        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        public byte[] ToBytes() => MessagePackSerializer.Serialize(Data, MessagePack.Resolvers.ContractlessStandardResolver.Options);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Utilities.DeserializeMessagePack<Internal>(bytes, out int bytesRead, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            Data.Sanitize();
            ConfigUpdated?.Invoke();
            return bytes.Slice(bytesRead);
        }

        // Apply
        public unsafe void Apply()
        {
            *Sewer56.SonicRiders.API.Misc.ResolutionX = Data.ResolutionX;
            *Sewer56.SonicRiders.API.Misc.ResolutionY = Data.ResolutionY;

            if (Data.Fullscreen == true)
                *Sewer56.SonicRiders.API.Misc.MultiSampleType = 0;

            *Sewer56.SonicRiders.API.Misc.Fullscreen = Data.Fullscreen;
            *Sewer56.SonicRiders.API.Misc.Blur = Data.Blur;

            // ResetDevice();
            ChangeBorderless(Data.Borderless);
        }

        public unsafe void ChangeBorderless(bool borderless)
        {
            // Reset Game Window
            var handle = Sewer56.SonicRiders.API.Window.WindowHandle;
            if (handle != IntPtr.Zero)
            {
                var style = GetWindowLongAuto(handle, WindowLongFlags.GWL_STYLE);

                if (style == IntPtr.Zero) 
                    return;

                var flags = (WindowStyles) style;
                if (borderless) 
                    RemoveBorder(ref flags);
                else
                    AddBorder(ref flags);

                SetWindowLong(handle, WindowLongFlags.GWL_STYLE, (int) flags);
                Task.Delay(100).ContinueWith((x) => ResizeWindow(Data.ResolutionX, Data.ResolutionY, handle));
            }
        }

        public void RemoveBorder(ref WindowStyles flags)
        {
            flags &= ~WindowStyles.WS_CAPTION;
            flags &= ~WindowStyles.WS_MAXIMIZEBOX;
            flags &= ~WindowStyles.WS_MINIMIZEBOX;
        }

        public void AddBorder(ref WindowStyles flags)
        {
            flags |= WindowStyles.WS_CAPTION;
            flags |= WindowStyles.WS_MAXIMIZEBOX;
            flags |= WindowStyles.WS_MINIMIZEBOX;
        }

        public unsafe void ResetDevice()
        {
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
                presentParametersCopy.BackBufferWidth = Data.ResolutionX;
                controller.Reset(controller.Dx9Device, ref presentParametersCopy);
            }
        }

        private void GetWindowSizeWithBorder(IntPtr handle, int x, int y, out int newX, out int newY)
        {
            // get size of window and the client area
            RECT clientRect = new RECT();
            GetWindowRect(handle, out var windowRect);
            GetClientRect(handle, ref clientRect);

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
                var monitor = MonitorFromWindow(handle, MonitorFlags.MONITOR_DEFAULTTONEAREST);
                var info = new MONITORINFO { cbSize = (uint)Struct.GetSize<MONITORINFO>() };

                if (GetMonitorInfo(monitor, ref info))
                {
                    left += (info.rcMonitor.Width - newX) / 2;
                    top += (info.rcMonitor.Height - newY) / 2;
                }
            }
            
            MoveWindow(handle, left, top, newX, newY, true);
        }

        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new TweaksEditorConfig();

        #region Internal
        public struct Internal
        {
            public bool BootToMenu;
            public bool FramePacing;
            public bool FramePacingSpeedup; // Speed up game to compensate for lag.
            public float DisableYieldThreshold;
            public bool D3DDeviceFlags;
            public bool DisableVSync;
            public bool AutoQTE;
            public int ResolutionX;
            public int ResolutionY;
            public bool Fullscreen;
            public bool Blur;
            public bool WidescreenHack;
            public bool Borderless;

            internal static Internal GetDefault() => new Internal
            {
                BootToMenu = true,
                FramePacingSpeedup = true,
                FramePacing = true,
                DisableYieldThreshold = 80,
                D3DDeviceFlags = true,
                DisableVSync = true,
                AutoQTE = true,
                ResolutionX = 1280,
                ResolutionY = 720,
                Fullscreen = false,
                Blur = false,
                WidescreenHack = false,
                Borderless = false
            };

            public void Sanitize()
            {
                if (ResolutionX <= 0 || ResolutionY <= 0)
                {
                    ResolutionX = 1024;
                    ResolutionY = 768;
                }
            }
        }
        #endregion
    }
}
