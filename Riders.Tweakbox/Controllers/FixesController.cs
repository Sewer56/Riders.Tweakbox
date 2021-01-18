using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Graphics;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.NumberUtilities.Matrices;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.NumberUtilities.Vectors;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Internal.DirectX;
using Sewer56.SonicRiders.Structures.Enums;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using static Sewer56.SonicRiders.API.Misc;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class FixesController
    {
        /// <summary>
        /// Amount of time spinning after sleep.
        /// </summary>
        public float TimerGranularity => (float) _fps.TimerGranularity;

        /// <summary>
        /// CPU Usage between 0 - 100.
        /// </summary>
        public float CpuUsage { get; private set; }

        public IntPtr DX9Device;
        public DX9Hook.Reset Reset;
        public IHook<DX9Hook.Reset> ResetHook;
        public PresentParameters PresentParameters;

        private static FixesController _controller;

        // Internal
        private bool _resetSpeedup = false;
        private Stopwatch _cpuLoadSampleWatch = Stopwatch.StartNew();
        private const float _cpuSampleIntervalMs = (float) ((1000 / 60.0f) * 10);
        private Device _device = new Device((IntPtr) 0x0);
        private EventController _event = IoC.Get<EventController>();

        // Settings
        private PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private TweaksEditorConfig _config = IoC.GetConstant<TweaksEditorConfig>();

        // Hooks
        private IHook<Functions.DefaultFn> _endFrameHook;
        private FramePacer _fps;
        private IAsmHook _bootToMenu;
        private IHook<TimeBeginPeriod> _beginPeriodHook;
        private IHook<TimeEndPeriod> _endPeriodHook;
        private IHook<DX9Hook.CreateDevice> _createDeviceHook;
        private IHook<Functions.DefaultReturnFn> _readConfigHook;
        private IHook<Functions.RenderTexture2DFnPtr> _renderTexture2dHook;
        private IHook<Functions.RenderPlayerIndicatorFnPtr> _renderPlayerIndicatorHook;
        private AspectConverter _aspectConverter = new AspectConverter(4 / 3f);
        private float _originalAspectRatio2dResX;
        
        public FixesController()
        {
            _controller = this;

            // Hook and disable frequency adjusting functions.
            var winmm = Native.LoadLibraryW("winmm.dll");
            var timeBeginPeriod = SDK.ReloadedHooks.CreateFunction<TimeBeginPeriod>((long) Native.GetProcAddress(winmm, "timeBeginPeriod"));
            var timeEndPeriod = SDK.ReloadedHooks.CreateFunction<TimeEndPeriod>((long) Native.GetProcAddress(winmm, "timeEndPeriod"));
            var beginEndPeriodPtr = (delegate*unmanaged[Stdcall]<int, int>)&TimeBeginEndPeriodImpl;
            _beginPeriodHook = timeBeginPeriod.Hook(Unsafe.AsRef<TimeBeginPeriod>((void*) &beginEndPeriodPtr)).Activate();
            _endPeriodHook = timeEndPeriod.Hook(Unsafe.AsRef<TimeEndPeriod>((void*) &beginEndPeriodPtr)).Activate();

            // Set Windows Timer resolution.
            Native.NtQueryTimerResolution(out int maximumResolution, out int _, out int currentResolution);
            Native.NtSetTimerResolution(maximumResolution, true, out currentResolution);

            // Now for our hooks.
            _createDeviceHook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.Direct3D9VTable.CreateFunctionHook<DX9Hook.CreateDevice>((int) IDirect3D9.CreateDevice, CreateDeviceHook).Activate();
            _readConfigHook = Functions.ReadConfigFile.Hook(ReadConfigFile).Activate();
            _originalAspectRatio2dResX = *AspectRatio2dResolutionX;
            
            var renderTexture2dPtr = (delegate*unmanaged[Stdcall]<int, Vector3*, int, float, int>)&RenderTexture2DPtr;
            _renderTexture2dHook = Functions.RenderTexture2DPtr.Hook(new Functions.RenderTexture2DFnPtr() { Value = renderTexture2dPtr }).Activate();

            var renderPlayerIndicatorPtr = (delegate*unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, int>)&RenderPlayerIndicatorPtr;
            _renderPlayerIndicatorHook = Functions.RenderPlayerIndicatorPtr.Hook(new Functions.RenderPlayerIndicatorFnPtr() { Value = renderPlayerIndicatorPtr }).Activate();
            _endFrameHook = Functions.EndFrame.Hook(EndFrameImpl).Activate();
            _fps = new FramePacer {FPSLimit = 60};

            if (_config.Data.BootToMenu)
            {
                var utils = SDK.ReloadedHooks.Utilities;
                var bootToMain = new string[]
                {
                    "use32",
                    $"{utils.AssembleAbsoluteCall(UnlockAllAndDisableBootToMenu, out _)}",
                    $"{utils.GetAbsoluteJumpMnemonics((IntPtr) 0x0046AF9D, false)}",
                };

                _bootToMenu = SDK.ReloadedHooks.CreateAsmHook(bootToMain, 0x0046AEE9, AsmHookBehaviour.ExecuteFirst).Activate();
            }

            _event.OnCheckIfQtePressLeft += EventOnOnCheckIfQtePressLeft;
            _event.OnCheckIfQtePressRight += EventOnOnCheckIfQtePressRight;
            Reset = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.DeviceVTable.CreateWrapperFunction<DX9Hook.Reset>((int)IDirect3DDevice9.Reset);
            ResetHook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.DeviceVTable.CreateFunctionHook<DX9Hook.Reset>((int)IDirect3DDevice9.Reset, ResetImpl);
        }

        private IntPtr ResetImpl(IntPtr device, ref PresentParameters presentparameters)
        {
            Log.WriteLine($"Reset! Device: {device}, Local Device {DX9Device}", LogCategory.Default);
            return ResetHook.OriginalFunction(device, ref presentparameters);
        }

        public void Disable()
        {
            _endFrameHook.Disable();
            _createDeviceHook.Disable();
            _bootToMenu.Disable();
            _beginPeriodHook.Disable();
            _endPeriodHook.Disable();
            _readConfigHook.Disable();
        }

        public void Enable()
        {
            _endFrameHook.Enable();
            _createDeviceHook.Enable();
            _bootToMenu.Enable();
            _beginPeriodHook.Enable();
            _endPeriodHook.Enable();
            _readConfigHook.Enable();
        }

        public void ResetSpeedup() => _resetSpeedup = true;
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressRight() => _config.Data.AutoQTE;
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressLeft() => _config.Data.AutoQTE;
        private int ReadConfigFile()
        {
            var originalResult = _readConfigHook.OriginalFunction();
            _config.Apply();
            return originalResult;
        }

        private void UnlockAllAndDisableBootToMenu()
        {
            // Unlock All
            for (var x = 0; x < State.UnlockedStages.Count; x++)
                State.UnlockedStages[x] = true;

            for (var x = 0; x < State.UnlockedCharacters.Count; x++)
                State.UnlockedCharacters[x] = true;

            var defaultModels = Enums.GetMembers<ExtremeGearModel>();
            for (var x = 0; x < State.UnlockedGearModels.Count; x++)
                if (defaultModels.Any(z => (int)z.Value == x))
                    State.UnlockedGearModels[x] = true;

            _bootToMenu.Disable();
        }

        /// <summary>
        /// Custom frame pacing implementation,
        /// </summary>
        private void EndFrameImpl()
        {
            // Sample CPU usage.
            if (_cpuLoadSampleWatch.Elapsed.TotalMilliseconds > _cpuSampleIntervalMs)
            {
                _cpuLoadSampleWatch.Restart();
                CpuUsage = _cpuCounter.NextValue();
            }

            if (_config.Data.FramePacing)
            {
                try
                {
                    var deviceAddress  = *(void**)0x016BF1B4;
                    _device.NativePointer = (IntPtr) deviceAddress;
                    _device.EndScene();
                }
                catch (Exception ex)
                {
                    /* Game is Stupid */
                }

                _fps.EndFrame(true, !_resetSpeedup && _config.Data.FramePacingSpeedup, CpuUsage < _config.Data.DisableYieldThreshold);
                *State.TotalFrameCounter += 1;

                if (_resetSpeedup)
                    _resetSpeedup = false;

                return;
            }

            _endFrameHook.OriginalFunction();
        }

        private IntPtr CreateDeviceHook(IntPtr direct3dpointer, uint adapter, DeviceType devicetype, IntPtr hfocuswindow, CreateFlags behaviorflags, ref PresentParameters presentParameters, int** ppreturneddeviceinterface)
        {
            if (_config.Data.D3DDeviceFlags)
            {
                behaviorflags &= ~CreateFlags.Multithreaded;
                behaviorflags |= CreateFlags.DisablePsgpThreading;
            }

            if (!presentParameters.Windowed)
                Native.ShowCursor(true);

            // Disable VSync
            if (_config.Data.DisableVSync)
            {
                presentParameters.PresentationInterval = PresentInterval.Immediate;
                presentParameters.FullScreenRefreshRateInHz = 0;
            }

            PresentParameters = presentParameters;
            var result = _createDeviceHook.OriginalFunction(direct3dpointer, adapter, devicetype, hfocuswindow, behaviorflags, ref presentParameters, ppreturneddeviceinterface);
            DX9Device = (IntPtr)(*ppreturneddeviceinterface);
            return result;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int RenderPlayerIndicatorPtr(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10) => _controller.RenderPlayerIndicator(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);

        private int RenderPlayerIndicator(int a1, int a2, int a3, int a4, int horizontalOffset, int a6, int a7, int a8, int a9, int a10)
        {
            if (_config.Data.WidescreenHack)
            {
                var actualAspect   = *ResolutionX / (float)*ResolutionY;
                var relativeAspect = (AspectConverter.GetRelativeAspect(actualAspect));

                // Get new screen width.
                var maximumX       = AspectConverter.GameCanvasWidth * relativeAspect;
                var borderLeft     = (_aspectConverter.GetBorderWidthX(actualAspect, AspectConverter.GameCanvasHeight) / 2);
                
                // Scale to new size of screen and offset (our RenderTexture2D Hook will re-add this offset!) 
                horizontalOffset = (int) (((horizontalOffset / AspectConverter.GameCanvasWidth) * maximumX) - borderLeft);
            }

            return _renderPlayerIndicatorHook.OriginalFunction.Value.Invoke(a1, a2, a3, a4, horizontalOffset, a6, a7, a8, a9, a10);
        }


        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int RenderTexture2DPtr(int isQuad, Vector3* vertices, int numVertices, float opacity) => _controller.RenderTexture2D(isQuad, vertices, numVertices, opacity);

        private int RenderTexture2D(int isQuad, Vector3* vertices, int numVertices, float opacity)
        {
            float Project(float original, float leftBorderOffset) => (leftBorderOffset + original);

            if (_config.Data.WidescreenHack)
            {
                // Update horizontal aspect.
                var currentAspectRatio = (float)*ResolutionX / *ResolutionY;
                *AspectRatio2dResolutionX = AspectConverter.GameCanvasWidth * (currentAspectRatio / (4f / 3f));

                // Get offset to shift vertices by.
                var actualAspect = *ResolutionX / (float)*ResolutionY;
                var leftBorderOffset = (_aspectConverter.GetBorderWidthX(actualAspect, *ResolutionY) / 2);

                // Try hack drawn 2d elements
                // Reimplemented based on inspecting RenderHud2dTextureInternal (0x004419D0) in disassembly.
                var vertexIsVector3 = (int*)0x17E51F8;
                if (*vertexIsVector3 == 1)
                {
                    if (numVertices >= 4)
                    {
                        int numMatrices = ((numVertices - 4) >> 2) + 1;
                        var matrix = (Matrix4x3<float, Float>*)vertices;
                        int totalMatVertices = numMatrices * 4;

                        for (int x = 0; x < numMatrices; x++)
                        {
                            matrix->X.X = Project(matrix->X.X, leftBorderOffset);
                            matrix->Y.X = Project(matrix->Y.X, leftBorderOffset);
                            matrix->Z.X = Project(matrix->Z.X, leftBorderOffset);
                            matrix->W.X = Project(matrix->W.X, leftBorderOffset);

                            matrix += 1; // Go to next matrix.
                        }

                        var extraVertices = numVertices - totalMatVertices;
                        var vertex = (Vector5<float, Float>*)matrix;
                        for (int x = 0; x < extraVertices; x++)
                        {
                            vertex->X = Project(vertex->X, leftBorderOffset);
                            vertex += 1;
                        }
                    }
                }
                else
                {
                    if (numVertices >= 4)
                    {
                        int numMatrices = ((numVertices - 4) >> 2) + 1;
                        var matrix = (Matrix4x5<float, Float>*)vertices;
                        int totalMatVertices = numMatrices * 4;

                        /*
                            The format of this matrix is strange
                            X X X X
                            Y Y Y Y
                            ? ? ? ?
                            ? ? ? ?
                            ? ? ? ?
                        */

                        for (int x = 0; x < numMatrices; x++)
                        {
                            matrix->X.X = Project(matrix->X.X, leftBorderOffset);
                            matrix->Y.X = Project(matrix->Y.X, leftBorderOffset);
                            matrix->Z.X = Project(matrix->Z.X, leftBorderOffset);
                            matrix->W.X = Project(matrix->W.X, leftBorderOffset);
                            matrix += 1; // Go to next matrix.
                        }

                        var extraVertices = numVertices - totalMatVertices;
                        var vertex = (Vector5<float, Float>*)matrix;
                        for (int x = 0; x < extraVertices; x++)
                        {
                            vertex->X = Project(vertex->X, leftBorderOffset);
                            vertex += 1;
                        }
                    }
                }
            }
            else
            {
                *AspectRatio2dResolutionX = _originalAspectRatio2dResX;
            }

            return _renderTexture2dHook.OriginalFunction.Value.Invoke(isQuad, vertices, numVertices, opacity);
        }

        /* Parameter: uMilliseconds */
        [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
        private static int TimeBeginEndPeriodImpl(int uMilliseconds) => 0;

        [Function(CallingConventions.Stdcall)]
        private struct TimeBeginPeriod { public FuncPtr<int, int> Value; }

        [Function(CallingConventions.Stdcall)]
        private struct TimeEndPeriod { public FuncPtr<int, int> Value; }
    }
}
