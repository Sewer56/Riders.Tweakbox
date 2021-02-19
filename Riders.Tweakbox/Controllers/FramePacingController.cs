﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using SharpDX.Direct3D9;
using Microsoft.Windows.Sdk;
using static Sewer56.SonicRiders.Functions.Functions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class FramePacingController : IController
    {
        private const float CpuSampleIntervalMs = (float)((1000 / 60.0f) * 10);
        private static FramePacingController _controller;

        /// <summary>
        /// An event executed before the <see cref="EndFrame"/> hook is executed.
        /// </summary>
        public event Action OnEndFrame;

        /// <summary>
        /// Amount of time spinning after sleep.
        /// </summary>
        public float TimerGranularity => (float)_fps.TimerGranularity;

        /// <summary>
        /// CPU Usage between 0 - 100.
        /// </summary>
        public float CpuUsage { get; private set; }

        /// <summary>
        /// The current Direct3D Device.
        /// </summary>
        private Device _device = new Device((IntPtr)0x0);

        /// <summary>
        /// Checks when to sample new CPU usage data.
        /// </summary>
        private Stopwatch _cpuLoadSampleWatch = Stopwatch.StartNew();

        private PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private FramePacer _fps;
        private bool _resetSpeedup = false;
        private TweaksEditorConfig _config = IoC.Get<TweaksEditorConfig>();

        // Hooks
        private IHook<TimeBeginPeriod> _beginPeriodHook;
        private IHook<TimeEndPeriod> _endPeriodHook;
        private IHook<ReturnVoidFnPtr> _endFrameHook;

        public FramePacingController()
        {
            // Hook and disable frequency adjusting functions.
            var winmm             = PInvoke.LoadLibrary("winmm.dll");
            var timeBeginPeriod   = SDK.ReloadedHooks.CreateFunction<TimeBeginPeriod>((long)Native.GetProcAddress(winmm, "timeBeginPeriod"));
            var timeEndPeriod     = SDK.ReloadedHooks.CreateFunction<TimeEndPeriod>((long)Native.GetProcAddress(winmm, "timeEndPeriod"));
            var beginEndPeriodPtr = (delegate* unmanaged[Stdcall]<int, int>)&TimeBeginEndPeriodImpl;
            _beginPeriodHook      = timeBeginPeriod.Hook(Unsafe.AsRef<TimeBeginPeriod>((void*)&beginEndPeriodPtr)).Activate();
            _endPeriodHook        = timeEndPeriod.Hook(Unsafe.AsRef<TimeEndPeriod>((void*)&beginEndPeriodPtr)).Activate();

            // Set Windows Timer resolution.
            Native.NtQueryTimerResolution(out int maximumResolution, out int _, out int currentResolution);
            Native.NtSetTimerResolution(maximumResolution, true, out currentResolution);

            // Test
            _endFrameHook = EndFrame.HookAs<ReturnVoidFnPtr>(typeof(FramePacingController), nameof(EndFrameImplStatic)).Activate();
            _fps          = new FramePacer { FPSLimit = 60 };
            _controller = this;
        }

        /// <inheritdoc />
        public void Disable()
        {
            _endFrameHook.Disable();
            _beginPeriodHook.Disable();
            _endPeriodHook.Disable();
        }

        /// <inheritdoc />
        public void Enable()
        {
            _endFrameHook.Enable();
            _beginPeriodHook.Enable();
            _endPeriodHook.Enable();
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
            // Invoke EndFrame event.
            OnEndFrame?.Invoke();

            // Sample CPU usage.
            if (_cpuLoadSampleWatch.Elapsed.TotalMilliseconds > CpuSampleIntervalMs)
            {
                _cpuLoadSampleWatch.Restart();
                CpuUsage = _cpuCounter.NextValue();
            }

            if (_config.Data.FramePacing)
            {
                try
                {
                    var deviceAddress = *(void**)0x016BF1B4;
                    _device.NativePointer = (IntPtr)deviceAddress;
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

            _endFrameHook.OriginalFunction.Value.Invoke();
        }

        /* Parameter: uMilliseconds */
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        public static int TimeBeginEndPeriodImpl(int uMilliseconds) => 0;

        [Function(CallingConventions.Stdcall)]
        private struct TimeBeginPeriod { public FuncPtr<int, int> Value; }

        [Function(CallingConventions.Stdcall)]
        private struct TimeEndPeriod { public FuncPtr<int, int> Value; }
    }
}
