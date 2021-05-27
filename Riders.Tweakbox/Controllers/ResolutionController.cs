using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Imgui.Hook;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Controllers
{
    public class ResolutionController : IController
    {
        private IHook<Functions.CdeclReturnIntFn> _readConfigHook;
        private TweaksConfig _config;
        private WindowService _windowService;
        private readonly ImguiHook _hook;
        private FramePacingController _pacingController;
        private IFunction<ResetDeviceFn> _resetDevice = SDK.Hooks.CreateFunction<ResetDeviceFn>(0x00519F70);
        private IFunction<SetupViewports> _setupViewports = SDK.Hooks.CreateFunction<SetupViewports>(0x0050F9E0);

        public ResolutionController(TweaksConfig config, WindowService service)
        {
            _config = config;
            _windowService = service;
            _readConfigHook = Functions.ReadConfigFile.Hook(ReadConfigFile).Activate();
            _config.Data.AddPropertyUpdatedHandler(OnPropertyUpdated);
        }

        private int ReadConfigFile()
        {
            var originalResult = _readConfigHook.OriginalFunction();
            Resize();
            SetFullscreen();
            SetBlur();
            return originalResult;
        }

        private unsafe void OnPropertyUpdated(string propertyname)
        {
            var data = _config.Data;
            switch (propertyname)
            {
                case nameof(data.ResolutionX):
                    Resize();
                    break;
                
                case nameof(data.ResolutionY):
                    Resize();
                    break;

                case nameof(data.Fullscreen):
                    //SetFullscreen();
                    break;

                // Blur effect is dependent on resolution; might tweak it one day.
                case nameof(_config.Data.Blur):
                    SetBlur();
                    break;
            }
        }

        private unsafe void SetBlur() => *Sewer56.SonicRiders.API.Misc.Blur = _config.Data.Blur;

        private unsafe void SetFullscreen()
        {
            var data = _config.Data;
            if (data.Fullscreen)
                *Sewer56.SonicRiders.API.Misc.MultiSampleType = 0;

            *Sewer56.SonicRiders.API.Misc.Fullscreen = data.Fullscreen;
        }

        private unsafe void Resize()
        {
            var data = _config.Data;
            *Sewer56.SonicRiders.API.Misc.ResolutionX = data.ResolutionX;
            *Sewer56.SonicRiders.API.Misc.ResolutionY = data.ResolutionY;
            ResetDevice();
        }

        public unsafe void ResetDevice()
        {
            // Currently Unused
            // Reset Game Window
            var handle = Sewer56.SonicRiders.API.Window.WindowHandle;
            var data = _config.Data;
            if (handle != IntPtr.Zero)
            {
                _pacingController ??= IoC.Get<FramePacingController>();
                _pacingController.AfterEndFrame += AfterEndFrame;
            }
        }

        private void AfterEndFrame()
        {
            _resetDevice.GetWrapper()();
            _setupViewports.GetWrapper()();
            if (Window.WindowHandle != IntPtr.Zero)
                _windowService.ResizeWindow(_config.Data.ResolutionX, _config.Data.ResolutionY, Window.WindowHandle);

            _pacingController.AfterEndFrame -= AfterEndFrame;
        }

        /// <summary>
        /// Internal game function to reset the D3D9 device.
        /// </summary>
        [Function(CallingConventions.Cdecl)]
        private delegate void ResetDeviceFn();

        /// <summary>
        /// Internal game function which sets up viewports for all players and menus.
        /// </summary>
        [Function(CallingConventions.Cdecl)]
        private delegate void SetupViewports();
    }
}
