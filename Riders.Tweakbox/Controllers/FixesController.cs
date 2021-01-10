using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Kernel32;
using Riders.Tweakbox.Components.FixesEditor;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using SharpDX.Direct3D9;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class FixesController
    {
        /// <summary>
        /// Amount of time spinning after sleep.
        /// </summary>
        public float SpinTime => _timerGranularityMs + 1;

        /// <summary>
        /// CPU Usage between 0 - 100.
        /// </summary>
        public float CpuUsage { get; private set; }

        /*
            The + 1 above is here because Sleep() has a granularity of 1.
            
            With a timer granularity of e.g. 0.5, this means the maximum time spent sleeping
            can be 1.5 for Sleep(1).
         
            Internally SharpFPS uses Sleep(1) until spinning, hence this decision is smart :)
        */

        // Internal
        private bool _resetSpeedup = false;
        private Stopwatch _cpuLoadSampleWatch = Stopwatch.StartNew();
        private const float _cpuSampleIntervalMs = (float)((1000 / 60.0f) * 10);
        private Device _device = new Device((IntPtr)0x0);

        // Settings
        private PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private FixesEditorConfig _config = IoC.GetConstant<FixesEditorConfig>();
        
        // Hooks
        private IHook<Functions.DefaultFn> _endFrameHook;
        private FramePacer _fps;
        private IAsmHook _bootToMenu;
        private float _timerGranularityMs;
        private IHook<TimeBeginPeriod> _beginPeriodHook;
        private IHook<TimeEndPeriod> _endPeriodHook;

        public FixesController()
        {
            // Hook and disable frequency adjusting functions.
            var winmm = Native.LoadLibraryW("winmm.dll");
            var timeBeginPeriod = SDK.ReloadedHooks.CreateFunction<TimeBeginPeriod>((long) Native.GetProcAddress(winmm, "timeBeginPeriod"));
            var timeEndPeriod   = SDK.ReloadedHooks.CreateFunction<TimeEndPeriod>((long) Native.GetProcAddress(winmm, "timeEndPeriod"));
            var beginEndPeriodPtr = (delegate*unmanaged[Stdcall]<int, int>)&TimeBeginEndPeriodImpl;
            _beginPeriodHook = timeBeginPeriod.Hook(Unsafe.AsRef<TimeBeginPeriod>((void*)&beginEndPeriodPtr)).Activate();
            _endPeriodHook = timeEndPeriod.Hook(Unsafe.AsRef<TimeEndPeriod>((void*)&beginEndPeriodPtr)).Activate();
            
            // Set Windows Timer resolution.
            Native.NtQueryTimerResolution(out int maximumResolution, out int _, out int currentResolution);
            Native.NtSetTimerResolution(maximumResolution, true, out currentResolution);
            _timerGranularityMs = currentResolution / 10000f; // 100us units to milliseconds.
            
            // Now for our 
            _endFrameHook = Functions.EndFrame.Hook(EndFrameImpl).Activate();
            _fps = new FramePacer
            {
                SpinTimeRemaining = 1,
                FPSLimit = 60
            };

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
        }

        public void Disable() => _endFrameHook.Disable();
        public void Enable()  => _endFrameHook.Enable();

        public void ResetSpeedup() => _resetSpeedup = true;

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
                catch (Exception)
                {
                    /* Game is Stupid */
                }

                _fps.SpinTimeRemaining = (float)SpinTime;
                _fps.EndFrame(true, !_resetSpeedup && _config.Data.FramePacingSpeedup, CpuUsage < _config.Data.DisableYieldThreshold);
                *State.TotalFrameCounter += 1;
                return;
            }

            _endFrameHook.OriginalFunction();
        }

        /* Parameter: uMilliseconds */
        [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
        static int TimeBeginEndPeriodImpl(int uMilliseconds) => 0;

        [Function(CallingConventions.Stdcall)]
        private struct TimeBeginPeriod { public FuncPtr<int, int> Value; }

        [Function(CallingConventions.Stdcall)]
        private struct TimeEndPeriod { public FuncPtr<int, int> Value; }
    }
}
