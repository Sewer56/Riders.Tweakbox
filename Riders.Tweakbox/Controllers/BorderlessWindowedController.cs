// ReSharper disable once RedundantUsingDirective
using Windows.Win32;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;
using Sewer56.SonicRiders.API;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using static Riders.Tweakbox.Misc.Native;
namespace Riders.Tweakbox.Controllers;

public class BorderlessWindowedController : IController
{
    private TweakboxConfig _config;
    private WindowService _windowService;
    private IHook<InitializeGameWindow> _initGameWindow;

    public BorderlessWindowedController(TweakboxConfig config, WindowService windowService, IReloadedHooks hooks)
    {
        _config = config;
        _windowService = windowService;

        _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
        _initGameWindow = hooks.CreateHook<InitializeGameWindow>(InitWindowImpl, 0x0051B800).Activate();
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
            _windowService.SetBorderless(borderless, handle);

        // Remove Border from Hardcoded Game style
        ref var hardcodedStyle = ref Unsafe.AsRef<WindowStyles>((void*)0x005119EC);
        _windowService.ToggleBorder(borderless, ref hardcodedStyle);
    }

    private int InitWindowImpl(IntPtr lpwindowname, IntPtr hinstance, int a3, IntPtr hmenu, int x, int y, int dwstyle, int xright, int ybottom)
    {
        var result = _initGameWindow.OriginalFunction(lpwindowname, hinstance, a3, hmenu, x, y, dwstyle, xright, ybottom);
        _windowService.SetBorderless(_config.Data.Borderless, Window.WindowHandle);
        return result;
    }

    [Function(CallingConventions.Cdecl)]
    public delegate int InitializeGameWindow(IntPtr lpWindowName, IntPtr hInstance, int a3, IntPtr hMenu, int x, int y, int dwStyle, int xRight, int yBottom);
}
