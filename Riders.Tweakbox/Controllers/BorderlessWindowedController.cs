// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;
using static Riders.Tweakbox.Misc.Native;

namespace Riders.Tweakbox.Controllers
{
    public class BorderlessWindowedController : IController
    {
        private TweaksConfig _config;
        private WindowService _windowService;

        public BorderlessWindowedController(TweaksConfig config, WindowService windowService)
        {
            _config = config;
            _windowService = windowService;

            _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
        }

        private void PropertyUpdated(string propertyname)
        {
            switch (propertyname)
            {
                case nameof(_config.Data.Borderless):
                    ChangeBorderless(_config.Data.Borderless);
                    break;
            }
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
                Task.Delay(100).ContinueWith((x) => _windowService.ResizeWindow(*Sewer56.SonicRiders.API.Misc.ResolutionX, *Sewer56.SonicRiders.API.Misc.ResolutionY, handle));
            }

            // Remove Border from Hardcoded Game style
            ref var hardcodedStyle = ref Unsafe.AsRef<WindowStyles>((void*) 0x005119EC);
            if (borderless)
                RemoveBorder(ref hardcodedStyle);
            else
                AddBorder(ref hardcodedStyle);
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
    }
}
