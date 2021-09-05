using System;
using System.Diagnostics;
using System.Linq;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX.Direct3D9;
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions.X86;
using SharpDX;
using Riders.Tweakbox.Configs;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Controllers;

public unsafe class Direct3DController : IController
{
    /// <summary>
    /// The D3D9 Instance.
    /// </summary>
    public Direct3DEx D3dEx { get; private set; }

    /// <summary>
    /// The D3D9 Device.
    /// </summary>
    public DeviceEx D3dDeviceEx { get; private set; }

    /// <summary>
    /// The current display modes.
    /// </summary>
    public DisplayModeCollection Modes { get; private set; }

    /// <summary>
    /// Last presentation parameters used for instantiation.
    /// </summary>
    public PresentParameters LastPresentParameters { get; private set; }

    private TweakboxConfig _config;
    private IFunction<Direct3dCreate9Wrapper> _d3dCreate9Wrapper = SDK.ReloadedHooks.CreateFunction<Direct3dCreate9Wrapper>(0x005253B0);

    // Hooks
    private IHook<DX9Hook.CreateDevice> _createDeviceHook;
    private IHook<DX9Hook.Reset> _resetHook;
    private IHook<Direct3dCreate9Wrapper> _createHook;
    private IHook<CreateVertexBuffer> _createVertexBufferHook;
    private IHook<CreateIndexBuffer> _createIndexBufferHook;
    private IHook<CreateTexture> _createTextureHook;
    private IHook<CreateVolumeTexture> _createVolumeTextureHook;
    private IHook<CreateCubeTexture> _createCubeTextureHook;
    private IHook<CreateOffscreenPlainSurface> _createOffscreenPlainSurfaceHook;

    public Direct3DController(TweakboxConfig config, IReloadedHooks hooks)
    {
        _config = config;
        var dx9Hook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value;
        _createDeviceHook = dx9Hook.Direct3D9VTable.CreateFunctionHook<DX9Hook.CreateDevice>((int)IDirect3D9.CreateDevice, CreateDeviceHook).Activate();
        _createTextureHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateTexture>((int)IDirect3DDevice9.CreateTexture, CreateTextureHook).Activate();
        _createVertexBufferHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateVertexBuffer>((int)IDirect3DDevice9.CreateVertexBuffer, CreateVertexBufferHook).Activate();
        _createIndexBufferHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateIndexBuffer>((int)IDirect3DDevice9.CreateIndexBuffer, CreateIndexBufferHook).Activate();
        _createVolumeTextureHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateVolumeTexture>((int)IDirect3DDevice9.CreateVolumeTexture, CreateVolumeTextureHook).Activate();
        _createCubeTextureHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateCubeTexture>((int)IDirect3DDevice9.CreateCubeTexture, CreateCubeTextureHook).Activate();
        _createOffscreenPlainSurfaceHook = dx9Hook.DeviceVTable.CreateFunctionHook<CreateOffscreenPlainSurface>((int)IDirect3DDevice9.CreateOffscreenPlainSurface, CreateOffscreenPlainSurfaceHook).Activate();
        _resetHook = dx9Hook.DeviceVTable.CreateFunctionHook<DX9Hook.Reset>((int)IDirect3DDevice9.Reset, ResetHook).Activate();
        _createHook = _d3dCreate9Wrapper.Hook(CreateRidersDeviceImpl).Activate();
    }

    /// <summary>
    /// Checks if the D3D device belongs to Riders.
    /// </summary>
    /// <param name="device">The native device pointer.</param>
    public bool IsRidersDevice(IntPtr device) => D3dDeviceEx != null && device == D3dDeviceEx.NativePointer;

    private IntPtr CreateRidersDeviceImpl(uint sdkversion)
    {
        D3dEx = new Direct3DEx();
        Modes = D3dEx.Adapters.First().GetDisplayModes(Format.X8R8G8B8);

        // Fill in method pointers.
        var moduleHandle = Native.GetModuleHandle("d3d9.dll");
        *(IntPtr*)0x016BEE8C = moduleHandle;

        *(IntPtr*)0x016BEE90 = Native.GetProcAddress(moduleHandle, "Direct3DCreate9");
        *(IntPtr*)0x016BEE94 = Native.GetProcAddress(moduleHandle, "D3DPERF_BeginEvent");
        *(IntPtr*)0x016BEE98 = Native.GetProcAddress(moduleHandle, "D3DPERF_EndEvent");
        *(IntPtr*)0x016BEE9C = Native.GetProcAddress(moduleHandle, "D3DPERF_SetMarker");
        *(IntPtr*)0x016BEEA0 = Native.GetProcAddress(moduleHandle, "D3DPERF_SetRegion");
        *(IntPtr*)0x016BEEA4 = Native.GetProcAddress(moduleHandle, "D3DPERF_QueryRepeatFrame");
        *(IntPtr*)0x016BEEA8 = Native.GetProcAddress(moduleHandle, "D3DPERF_SetOptions");
        *(IntPtr*)0x016BEEAC = Native.GetProcAddress(moduleHandle, "D3DPERF_GetStatus");

        return D3dEx.NativePointer;
    }

    private IntPtr CreateDeviceHook(IntPtr direct3dpointer, uint adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags, ref PresentParameters presentParameters, int** ppReturnedDeviceInterface)
    {
        // Do not edit if does not belong to Riders.
        if (D3dEx == null || direct3dpointer != D3dEx.NativePointer)
        {
            Log.WriteLine($"[{nameof(CreateDeviceHook)}] Not Riders Device, Ignoring");
            return _createDeviceHook.OriginalFunction(direct3dpointer, adapter, deviceType, hFocusWindow, behaviorFlags, ref presentParameters, ppReturnedDeviceInterface);
        }
        
        if (_config.Data.HardwareVertexProcessing)
            behaviorFlags |= CreateFlags.HardwareVertexProcessing;

        if (_config.Data.DisablePsgpThreading)
            behaviorFlags |= CreateFlags.DisablePsgpThreading;
        
        if (!presentParameters.Windowed)
            PInvoke.ShowCursor(true);

        // Disable VSync
        SetPresentParameters(ref presentParameters);

#if DEBUG
        PInvoke.SetWindowText(new HWND(Window.WindowHandle), $"Sonic Riders w/ Tweakbox (Debug) | PID: {Process.GetCurrentProcess().Id}");
#endif
        try
        {
            LastPresentParameters = presentParameters;
            if (presentParameters.Windowed)
            {
                D3dDeviceEx = new DeviceEx(D3dEx, (int)adapter, deviceType, hFocusWindow, behaviorFlags, presentParameters);
            }
            else
            {
                D3dDeviceEx = new DeviceEx(D3dEx, (int)adapter, deviceType, hFocusWindow, behaviorFlags, presentParameters, new DisplayModeEx()
                {
                    Format = presentParameters.BackBufferFormat,
                    Height = presentParameters.BackBufferHeight,
                    Width = presentParameters.BackBufferWidth,
                    RefreshRate = presentParameters.FullScreenRefreshRateInHz,
                    ScanLineOrdering = ScanlineOrdering.Progressive,
                });
            }

            IoC.Kernel.Bind<Device>().ToConstant(D3dDeviceEx);
            *ppReturnedDeviceInterface = (int*)D3dDeviceEx.NativePointer;
        }
        catch (SharpDXException ex)
        {
            Log.WriteLine($"Failed To Initialize Direct3DEx Device: HRESULT | {ex.HResult}, Descriptor | {ex.Descriptor}");
            return (IntPtr)ex.HResult;
        }

        return IntPtr.Zero;
    }

    private IntPtr ResetHook(IntPtr device, ref PresentParameters presentparameters)
    {
        if (D3dDeviceEx == null || D3dDeviceEx.NativePointer != device)
        {
            Log.WriteLine($"[{nameof(ResetHook)}] Not Riders Device, Ignoring");
            return _resetHook.OriginalFunction(device, ref presentparameters);
        }

        SetPresentParameters(ref presentparameters);
        LastPresentParameters = presentparameters;
        return _resetHook.OriginalFunction(device, ref presentparameters);
    }

    private void SetPresentParameters(ref PresentParameters presentParameters)
    {
        if (_config.Data.DisableVSync)
        {
            presentParameters.PresentationInterval = PresentInterval.Immediate;
            presentParameters.FullScreenRefreshRateInHz = 0;
        }
        
        // Cannot be used with FlipEx
        presentParameters.Windowed = !_config.Data.Fullscreen;
        presentParameters.MultiSampleQuality = 0;
        presentParameters.MultiSampleType = 0;

        // Subscribe to new D3D9Ex Swap Model
        if (!_config.Data.Fullscreen)
        {
            presentParameters.BackBufferCount = 2;
            presentParameters.SwapEffect = SwapEffect.FlipEx;
        }

        // Set new resolution.
        presentParameters.BackBufferWidth = _config.Data.ResolutionX;
        presentParameters.BackBufferHeight = _config.Data.ResolutionY;
    }

    #region Hooks for D3D9Ex Support
    private IntPtr CreateTextureHook(IntPtr devicePointer, int width, int height, int miplevels, Usage usage, Format format, Pool pool, void** pptexture, void* sharedhandle)
    {
        usage |= (pool == Pool.Managed ? Usage.Dynamic : 0);
        pool = (pool == Pool.Managed) ? Pool.Default : pool;
        return _createTextureHook.OriginalFunction(devicePointer, width, height, miplevels, usage, format, pool, pptexture, sharedhandle);
    }

    private IntPtr CreateVertexBufferHook(IntPtr devicePointer, uint length, Usage usage, VertexFormat format, Pool pool, void** ppvertexbuffer, void* psharedhandle)
    {
        usage |= (pool == Pool.Managed ? Usage.Dynamic : 0);
        pool = (pool == Pool.Managed) ? Pool.Default : pool;
        return _createVertexBufferHook.OriginalFunction(devicePointer, length, usage, format, pool, ppvertexbuffer, psharedhandle);
    }

    private IntPtr CreateIndexBufferHook(IntPtr devicePointer, uint length, Usage usage, Format format, Pool pool, void** ppindexbuffer, void* psharedhandle)
    {
        usage |= (pool == Pool.Managed ? Usage.Dynamic : 0);
        pool = (pool == Pool.Managed) ? Pool.Default : pool;

        return _createIndexBufferHook.OriginalFunction(devicePointer, length, usage, format, pool, ppindexbuffer, psharedhandle);
    }
    #endregion

    #region D3D9Ex: These APIs aren't used but just in case!!
    private IntPtr CreateVolumeTextureHook(IntPtr devicepointer, int width, int height, int depth, int miplevels, Usage usage, Format format, Pool pool, void** pptexture, void* sharedhandle)
    {
        usage |= (pool == Pool.Managed ? Usage.Dynamic : 0);
        pool = (pool == Pool.Managed) ? Pool.Default : pool;
        return _createVolumeTextureHook.OriginalFunction(devicepointer, width, height, depth, miplevels, usage, format, pool, pptexture, sharedhandle);
    }

    private IntPtr CreateCubeTextureHook(IntPtr devicepointer, int edgelength, int levels, Usage usage, Format format, Pool pool, void** pptexture, void* sharedhandle)
    {
        usage |= (pool == Pool.Managed ? Usage.Dynamic : 0);
        pool = (pool == Pool.Managed) ? Pool.Default : pool;
        return _createCubeTextureHook.OriginalFunction(devicepointer, edgelength, levels, usage, format, pool, pptexture, sharedhandle);
    }

    private IntPtr CreateOffscreenPlainSurfaceHook(IntPtr devicepointer, int width, int height, Format format, Pool pool, void** ppsurface, void* sharedhandle)
    {
        pool = (pool == Pool.Managed) ? Pool.Default : pool;
        return _createOffscreenPlainSurfaceHook.OriginalFunction(devicepointer, width, height, format, pool, ppsurface, sharedhandle);
    }
    #endregion

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    private delegate IntPtr Direct3dCreate9Wrapper(uint sdkVersion);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateVertexBuffer(IntPtr devicePointer, uint length, Usage usage, VertexFormat format, Pool pool, void** ppVertexBuffer, void* pSharedHandle);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateIndexBuffer(IntPtr devicePointer, uint length, Usage usage, Format format, Pool pool, void** ppIndexBuffer, void* pSharedHandle);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateTexture(IntPtr devicePointer, int width, int height, int miplevels, Usage usage, Format format, Pool pool, void** ppTexture, void* sharedHandle);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateVolumeTexture(IntPtr devicePointer, int width, int height, int depth, int miplevels, Usage usage, Format format, Pool pool, void** ppTexture, void* sharedHandle);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateCubeTexture(IntPtr devicePointer, int edgeLength, int levels, Usage usage, Format format, Pool pool, void** ppTexture, void* sharedHandle);

    [FunctionHookOptions(PreferRelativeJump = true)]
    [Function(CallingConventions.Stdcall)]
    public unsafe delegate IntPtr CreateOffscreenPlainSurface(IntPtr devicePointer, int width, int height, Format format, Pool pool, void** ppSurface, void* sharedHandle);
}
