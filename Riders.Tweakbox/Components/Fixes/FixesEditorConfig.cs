using System;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX.Direct3D9;
using Vanara.PInvoke;

namespace Riders.Tweakbox.Components.Fixes
{
    public class FixesEditorConfig : IConfiguration
    {
        private static DX9Hook.Reset Reset;


        public Internal Data = Internal.GetDefault();
        
        // Serialization
        public byte[] ToBytes() => Json.SerializeStruct(ref Data);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Json.DeserializeStruct<Internal>(bytes);
            Data.Sanitize();
            return bytes.Slice(Struct.GetSize<Internal>());
        }

        // Apply
        public unsafe void Apply()
        {
            *Sewer56.SonicRiders.API.Misc.ResolutionX = Data.ResolutionX;
            *Sewer56.SonicRiders.API.Misc.ResolutionY = Data.ResolutionY;

            *Sewer56.SonicRiders.API.Misc.Blur       = Data.Blur;
            if (Data.Fullscreen == true)
                *Sewer56.SonicRiders.API.Misc.MultiSampleType = 0;

            *Sewer56.SonicRiders.API.Misc.Fullscreen = Data.Fullscreen;

            // Reset Game Window
            var handle = Sewer56.SonicRiders.API.Window.WindowHandle;
            if (handle != IntPtr.Zero)
            {
                // Resize Window
                User32_Gdi.MoveWindow(new HWND(handle), 0, 0, Data.ResolutionX, Data.ResolutionY, true);

                // Reset D3D Device
                // TODO: Write code to recreate all textures and possibly other assets, as described in Reset() function.
                var controller = IoC.Get<FixesController>();
                var presentParametersCopy = controller.PresentParameters;
                presentParametersCopy.BackBufferHeight = Data.ResolutionY;
                presentParametersCopy.BackBufferWidth  = Data.ResolutionX;
                controller.Reset(controller.DX9Device, ref presentParametersCopy);
            }
        }

        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new FixesEditorConfig();

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
                WidescreenHack = false
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
