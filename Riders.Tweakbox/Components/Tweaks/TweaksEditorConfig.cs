using System;
using MessagePack;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Definitions.Serializers;
using static Riders.Tweakbox.Misc.Native;
using Task = System.Threading.Tasks.Task;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Riders.Tweakbox.Definitions.Serializers.MessagePack;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditorConfig : IConfiguration
    {
        private static IFormatterResolver MsgPackResolver = MessagePack.Resolvers.CompositeResolver.Create(NullableResolver.Instance, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        private static MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(MsgPackResolver);

        // Serialization
        public Internal Data = Internal.GetDefault();

        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        public byte[] ToBytes() => MessagePackSerializer.Serialize(Data, SerializerOptions);
        public void FromBytes(Span<byte> bytes)
        {
            Data = Utilities.DeserializeMessagePack<Internal>(bytes, out int bytesRead, SerializerOptions);
            Data.Initialize();
            ConfigUpdated?.Invoke();
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

            //ResetDevice();
            ChangeBorderless(Data.Borderless);
            ConfigUpdated?.Invoke();
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
                Task.Delay(100).ContinueWith((x) => ResizeWindow(Data.ResolutionX, Data.ResolutionY, handle));
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

        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new TweaksEditorConfig();

        #region Internal
        public struct Internal
        {
            public Definitions.Nullable<bool> BootToMenu;
            public Definitions.Nullable<bool> FramePacing;
            public Definitions.Nullable<bool> FramePacingSpeedup; // Speed up game to compensate for lag.
            public Definitions.Nullable<float> DisableYieldThreshold;
            public Definitions.Nullable<bool> D3DDeviceFlags;
            public Definitions.Nullable<bool> DisableVSync;
            public Definitions.Nullable<bool> AutoQTE;
            public Definitions.Nullable<int> ResolutionX;
            public Definitions.Nullable<int> ResolutionY;
            public Definitions.Nullable<bool> Fullscreen;
            public Definitions.Nullable<bool> Blur;
            public Definitions.Nullable<bool> WidescreenHack;
            public Definitions.Nullable<bool> Borderless;
            public Definitions.Nullable<bool> SinglePlayerStageData;
            public Definitions.Nullable<bool> SinglePlayerModels;

            internal static Internal GetDefault()
            {
                var result = new Internal();
                result.Initialize();
                return result;
            }

            public void Initialize()
            {
                BootToMenu.SetIfNull(true);
                FramePacingSpeedup.SetIfNull(true);
                FramePacing.SetIfNull(true);
                DisableYieldThreshold.SetIfNull(80);
                D3DDeviceFlags.SetIfNull(true);
                DisableVSync.SetIfNull(true);
                AutoQTE.SetIfNull(true);
                ResolutionX.SetIfNull(1280);
                ResolutionY.SetIfNull(720);
                Fullscreen.SetIfNull(false);
                Blur.SetIfNull(false);
                WidescreenHack.SetIfNull(false);
                Borderless.SetIfNull(false);
                SinglePlayerStageData.SetIfNull(true);
                SinglePlayerModels.SetIfNull(true);
            }
        }
        #endregion
    }
}
