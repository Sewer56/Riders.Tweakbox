using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Various patches for pushing the game a bit more towards the limit.
    /// </summary>
    public class LimitBreakController : IController
    {
        public unsafe LimitBreakController(IReloadedHooks hooks)
        {
            // 2GB Heap
            var characteristics    = (short*)0x40013E;
            bool largeAddressAware = (*characteristics & 0x20) != 0;

            // Unmanaged Heap
            // 3000000 (50MB) -> 7A120000 (2GB)
            int heapSize = 0x7A120000;
            if (!largeAddressAware)
            {
                Log.WriteLine($"EXE is not Large Address Aware, Setting Heap as 768MB.");
                heapSize = 0x2DC6C000;
            }
            else
            {
                Log.WriteLine($"EXE is Large Address Aware, Setting Heap as 2048MB.");
            }

            *(int*) 0x527C24 = heapSize;
            *(int*) 0x527C5E = heapSize;

            // Extend Task Heap
            // 1024 -> 40960 Total Tasks
            // 200 -> 8000 Task Heap 1
            // 800 -> 32000 Task Heap 2
            *(int*) 0x4186FC = 40960;
            *(int*) 0x4186F7 = 8000;
            *(int*) 0x4186CF = 32000;

            // 8192 Collision Object Limit (from 600)
            *(int*) 0x441954 = 0x420000; // Memory Allocated
            *(int*) 0x4419AD = 0x2000; // Init Loop Iterations
            *(int*) 0x441990 = 0x2000; // Init Loop Iterations
        }
    }
}
