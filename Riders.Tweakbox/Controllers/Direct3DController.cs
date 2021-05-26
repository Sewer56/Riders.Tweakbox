using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX.Direct3D9;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using SharpDX;
using Riders.Tweakbox.Configs;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class Direct3DController : IController
    {
        /// <summary>
        /// The D3D9 Instance.
        /// </summary>
        public Direct3D D3d { get; private set; }

        /// <summary>
        /// The D3D9 Device.
        /// </summary>
        public DeviceEx D3dDeviceEx { get; private set; }

        /// <summary>
        /// Presentation parameters passed to Direct3D on last device reset.
        /// </summary>
        public PresentParameters LastPresentParameters;

        private TweaksConfig _config;

        // Hooks
        private IHook<DX9Hook.CreateDevice> _createDeviceHook;

        public Direct3DController(TweaksConfig config)
        {
            _config = config;
            _createDeviceHook  = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.Direct3D9VTable.CreateFunctionHook<DX9Hook.CreateDevice>((int)IDirect3D9.CreateDevice, CreateDeviceHook).Activate();
        }

        private IntPtr CreateDeviceHook(IntPtr direct3dpointer, uint adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags, ref PresentParameters presentParameters, int** ppReturnedDeviceInterface)
        {
            if (_config.Data.D3DDeviceFlags)
            {
                behaviorFlags &= ~CreateFlags.Multithreaded;
                behaviorFlags |= CreateFlags.DisablePsgpThreading;
            }

            if (!presentParameters.Windowed)
                PInvoke.ShowCursor(true);

            // Disable VSync
            if (_config.Data.DisableVSync)
            {
                presentParameters.PresentationInterval = PresentInterval.Immediate;
                presentParameters.FullScreenRefreshRateInHz = 0;
            }

#if DEBUG
            PInvoke.SetWindowText(new HWND(Window.WindowHandle), $"Sonic Riders w/ Tweakbox (Debug) | PID: {Process.GetCurrentProcess().Id}");
#endif
            LastPresentParameters = presentParameters;
            try
            {
                D3d = new Direct3D(direct3dpointer);
                if (presentParameters.Windowed)
                {
                    D3dDeviceEx = new DeviceEx(new Direct3DEx(direct3dpointer), (int)adapter, deviceType, hFocusWindow, behaviorFlags, presentParameters);
                }
                else
                {
                    D3dDeviceEx = new DeviceEx(new Direct3DEx(direct3dpointer), (int)adapter, deviceType, hFocusWindow, behaviorFlags, presentParameters, new DisplayModeEx()
                    {
                        Format = presentParameters.BackBufferFormat,
                        Height = presentParameters.BackBufferHeight,
                        Width = presentParameters.BackBufferWidth,
                        RefreshRate = presentParameters.FullScreenRefreshRateInHz,
                        ScanLineOrdering = ScanlineOrdering.Progressive,
                    });
                }
                
                *ppReturnedDeviceInterface = (int*)D3dDeviceEx.NativePointer;
            }
            catch (SharpDXException ex)
            {
                Log.WriteLine($"Failed To Initialize Direct3DEx Device: HRESULT | {ex.HResult}, Descriptor | {ex.Descriptor}");
                return (IntPtr) ex.HResult;
            }

            return IntPtr.Zero;
        }
    }
}
