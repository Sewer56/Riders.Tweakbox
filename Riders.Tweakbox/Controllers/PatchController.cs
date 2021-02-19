using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Controller that contains minor patches to game code.
    /// </summary>
    public class PatchController : IController
    {
        /// <summary>
        /// If enabled, adds code that keeps the lap timer running even after the race finishes.
        /// </summary>
        public IAsmHook InjectRunTimerPostRace;

        /// <summary>
        /// Disables overwriting of race position after the race has completed.
        /// </summary>
        public Patch DisableRacePositionOverwrite = new Patch((IntPtr)0x4B40E6, new byte[] { 0xEB, 0x44 });

        public PatchController()
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var runTimerPostRace = new string[]
            {
                "use32",
                "lea eax, dword [ebp+8]",
                "push 0x00692AE0",
                "push eax",
                $"{utilities.GetAbsoluteCallMnemonics((IntPtr) 0x00414F00, false)}",
                "add esp, 8"
            };

            InjectRunTimerPostRace = SDK.ReloadedHooks.CreateAsmHook(runTimerPostRace, 0x004166EB).Activate();
        }

        /// <inheritdoc />
        public void Disable()
        {
            InjectRunTimerPostRace.Disable();
            DisableRacePositionOverwrite.Disable();
        }

        /// <inheritdoc />
        public void Enable()
        {
            InjectRunTimerPostRace.Enable();
            DisableRacePositionOverwrite.Enable();
        }
    }
}
