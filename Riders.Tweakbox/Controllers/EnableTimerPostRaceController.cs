using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;

namespace Riders.Tweakbox.Controllers
{
    public class EnableTimerPostRaceController : IController
    {
        /// <summary>
        /// If enabled, adds code that keeps the lap timer running even after the race finishes.
        /// </summary>
        public IAsmHook Hook;

        public EnableTimerPostRaceController(IReloadedHooks hooks, IReloadedHooksUtilities utils)
        {
            var runTimerPostRace = new string[]
            {
                "use32",
                "lea eax, dword [ebp+8]",
                "push 0x00692AE0",
                "push eax",
                $"{utils.GetAbsoluteCallMnemonics((IntPtr) 0x00414F00, false)}",
                "add esp, 8"
            };

            Hook = hooks.CreateAsmHook(runTimerPostRace, 0x004166EB).Activate();
        }
    }
}
