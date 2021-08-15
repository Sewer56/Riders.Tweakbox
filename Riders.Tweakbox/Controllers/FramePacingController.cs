using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using static Sewer56.SonicRiders.Functions.Functions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Microsoft.Windows.Sdk;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc.Extensions;
using Task = System.Threading.Tasks.Task;

namespace Riders.Tweakbox.Controllers;

public unsafe class FramePacingController : IController
{
    private const float CpuSampleIntervalMs = (float)((1000 / 60.0f) * 10);
    private static FramePacingController _controller;

    /// <summary>
    /// An action to run after the frame finishes rendering.
    /// </summary>
    public Action AfterEndFrame { get; set; }

    /// <summary>
    /// Amount of time spinning after sleep.
    /// </summary>
    public float TimerGranularity => (float)Fps.TimerGranularity;

    /// <summary>
    /// CPU Usage between 0 - 100.
    /// </summary>
    public float CpuUsage { get; private set; }

    /// <summary>
    /// Provides access to performance statistics.
    /// </summary>
    public FramePacer Fps { get; private set; }

    /// <summary>
    /// Checks when to sample new CPU usage data.
    /// </summary>
    private Stopwatch _cpuLoadSampleWatch = Stopwatch.StartNew();

    private PerformanceCounter _cpuCounter;

    private bool _resetSpeedup = false;
    private TweakboxConfig _config = IoC.Get<TweakboxConfig>();
    private bool _initialized = false;

    // Hooks
    private IHook<TimeBeginPeriod> _beginPeriodHook;
    private IHook<TimeEndPeriod> _endPeriodHook;
    private IHook<ReturnVoidFnPtr> _endFrameHook;
    private Direct3DController _direct3DController = IoC.GetSingleton<Direct3DController>();

    public FramePacingController()
    {
        // Set this field in background because it's slow and blocking.
        try { Task.Run(() => _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total")).ConfigureAwait(false); }
        catch (Exception e) { /* Fails on some machines. */ }

        // Hook and disable frequency adjusting functions.
        var winmm = PInvoke.LoadLibrary("winmm.dll");
        var timeBeginPeriod = SDK.ReloadedHooks.CreateFunction<TimeBeginPeriod>((long)Native.GetProcAddress(winmm, "timeBeginPeriod"));
        var timeEndPeriod = SDK.ReloadedHooks.CreateFunction<TimeEndPeriod>((long)Native.GetProcAddress(winmm, "timeEndPeriod"));
        var beginEndPeriodPtr = (delegate* unmanaged[Stdcall]<int, int>)&TimeBeginEndPeriodImpl;
        _beginPeriodHook = timeBeginPeriod.Hook(Unsafe.AsRef<TimeBeginPeriod>((void*)&beginEndPeriodPtr)).Activate();
        _endPeriodHook = timeEndPeriod.Hook(Unsafe.AsRef<TimeEndPeriod>((void*)&beginEndPeriodPtr)).Activate();

        // Set Windows Timer resolution.
        Native.NtQueryTimerResolution(out int maximumResolution, out int _, out int currentResolution);
        Native.NtSetTimerResolution(maximumResolution, true, out currentResolution);

        // Test
        _endFrameHook = EndFrame.HookAs<ReturnVoidFnPtr>(typeof(FramePacingController), nameof(EndFrameImplStatic)).Activate();
        Fps = new FramePacer { FPSLimit = 60 };
        _controller = this;
    }

    /// <summary>
    /// If called, resets the speedup "lag compensation" inside the frame pacing implementation.
    /// </summary>
    public void ResetSpeedup() => _resetSpeedup = true;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static void EndFrameImplStatic() => _controller.EndFrameImpl();

    /// <summary>
    /// Custom frame pacing implementation
    /// </summary>
    private void EndFrameImpl()
    {
        // Setup support for UI Components
        if (!_initialized)
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            _initialized = true;
        }

        // Sample CPU usage.
        if (_cpuLoadSampleWatch.Elapsed.TotalMilliseconds > CpuSampleIntervalMs)
        {
            _cpuLoadSampleWatch.Restart();
            if (_cpuCounter != null)
                CpuUsage = _cpuCounter.NextValue();
        }

        // Seed number generator.
        SharedRandom.Instance.Next();
        if (_config.Data.FramePacing)
        {
            try
            {
                _direct3DController.D3dDeviceEx.EndScene();
            }
            catch (Exception ex)
            {
                /* Game is Stupid */
            }

            Fps.EndFrame(_config.Data.MaxSpeedupTimeMillis, true, !_resetSpeedup && _config.Data.FramePacingSpeedup, CpuUsage < _config.Data.DisableYieldThreshold, _config.Data.RemoveFpsCap);
            *State.TotalFrameCounter += 1;

            if (_resetSpeedup)
                _resetSpeedup = false;

            AfterEndFrame?.Invoke();
            return;
        }

        _endFrameHook.OriginalFunction.Value.Invoke();
        AfterEndFrame?.Invoke();
    }

    /* Parameter: uMilliseconds */
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static int TimeBeginEndPeriodImpl(int uMilliseconds) => 0;

    [Function(CallingConventions.Stdcall)]
    private struct TimeBeginPeriod { public FuncPtr<int, int> Value; }

    [Function(CallingConventions.Stdcall)]
    private struct TimeEndPeriod { public FuncPtr<int, int> Value; }
}
