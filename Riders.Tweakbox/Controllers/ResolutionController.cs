using System;
using Reloaded.Hooks.Definitions;
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
                    //Resize();
                    break;
                
                case nameof(data.ResolutionY):
                    //Resize();
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
            if (Window.WindowHandle != IntPtr.Zero)
                _windowService.ResizeWindow(_config.Data.ResolutionX, _config.Data.ResolutionY, Window.WindowHandle);

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
                // Reset D3D Device
                // TODO: Write code to recreate all textures and possibly other assets, as described in Reset() function.
                var controller = IoC.Get<Direct3DController>();
                var presentParametersCopy = controller.LastPresentParameters;
                presentParametersCopy.BackBufferHeight = data.ResolutionY;
                presentParametersCopy.BackBufferWidth  = data.ResolutionX;
                controller.D3dDeviceEx.ResetEx(ref presentParametersCopy);
            }
        }
    }
}
