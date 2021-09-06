using Riders.Tweakbox.Controllers.Interfaces;
using System;
using Reloaded.Memory.Sources;

namespace Riders.Tweakbox.Controllers
{
    public class StartupRestrictionKill : IController
    {
        private static byte[] _jmpSkipLauncher = { 0xEB, 0x23 }; // jmp 0x25
        private static byte[] _jmpSkipOneInstance = { 0xEB, 0x28 }; // jmp 0x2A

        public StartupRestrictionKill()
        {
            // Ignore launcher check result.
            Memory.CurrentProcess.SafeWriteRaw((IntPtr)0x005118CF, _jmpSkipLauncher);

            // Ignore only one instance check result.
            Memory.CurrentProcess.SafeWriteRaw((IntPtr)0x0051190F, _jmpSkipOneInstance);
        }
    }
}
