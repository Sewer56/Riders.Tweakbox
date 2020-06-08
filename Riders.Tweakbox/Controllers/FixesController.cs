using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Interop;
using Reloaded.WPF.Animations.FrameLimiter;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using SharpDX.Direct3D9;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class FixesController
    {
        // Settings
        public Pinnable<byte> FramePacing { get; private set; } = new Pinnable<byte>(1);
        public Pinnable<byte> SpinTime { get; private set; } = new Pinnable<byte>(1);

        // Hooks
        private IHook<Functions.DefaultFn> _endFrameHook;
        private SharpFPS _fps;

        public FixesController()
        {
            _endFrameHook = Functions.EndFrame.Hook(CustomFramePacing).Activate();
            _fps = new SharpFPS
            {
                SpinTimeRemaining = 1,
                FPSLimit = 60
            };
        }

        public void Disable() { _endFrameHook.Disable(); }
        public void Enable() { _endFrameHook.Enable(); }

        /// <summary>
        /// Custom frame pacing implementation,
        /// </summary>
        private void CustomFramePacing()
        {
            if (FramePacing.Value == 1)
            {
                try
                {
                    var deviceAddy  = *(void**)0x016BF1B4;
                    var device      = new Device((IntPtr)(deviceAddy));
                    device.EndScene();
                }
                catch (Exception)
                {
                    /* Game is Stupid */
                }

                _fps.SpinTimeRemaining = (float) SpinTime.Value;
                _fps.EndFrame(true);
                *State.TotalFrameCounter += 1;
                return;
            }

            _endFrameHook.OriginalFunction();
        }


    }
}
