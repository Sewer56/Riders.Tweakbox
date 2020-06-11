using System;
using System.Linq;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Reloaded.WPF.Animations.FrameLimiter;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
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
        private IAsmHook _bootToMenu;
        
        public FixesController()
        {
            _endFrameHook = Functions.EndFrame.Hook(CustomFramePacing).Activate();
            _fps = new SharpFPS
            {
                SpinTimeRemaining = 1,
                FPSLimit = 60
            };

            var utils = SDK.ReloadedHooks.Utilities;
            var bootToMain = new string[]
            {
                "use32",
                $"{AsmHelpers.AssembleAbsoluteCall(UnlockAllAndDisableBootToMenu, out _)}",
                $"{utils.GetAbsoluteJumpMnemonics((IntPtr) 0x0046AF9D, false)}",
            };
            
            _bootToMenu = SDK.ReloadedHooks.CreateAsmHook(bootToMain, 0x0046AEE9, AsmHookBehaviour.ExecuteFirst).Activate();
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
