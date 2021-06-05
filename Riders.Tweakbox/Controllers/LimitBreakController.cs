using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Various patches for pushing the game a bit more towards the limit.
    /// </summary>
    public class LimitBreakController : IController
    {
        private IHook<Functions.FreeFnPtr> _freeFrameHook;
        private static LimitBreakController _this;

        public unsafe LimitBreakController(IReloadedHooks hooks)
        {
            _this = this;

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

            _freeFrameHook = Functions.FreeFrame.HookAs<Functions.FreeFnPtr>(typeof(LimitBreakController), nameof(FreeFrameStatic)).Activate();
        }

        private unsafe BlittablePointer<byte> FreeFrame(void* address)
        {
            uint currentHeader = *(uint*)0x017B8DA8;
            uint newHeader     = (uint) address;
            int bytesFreed     = (int) (currentHeader - newHeader); 

            var result = _freeFrameHook.OriginalFunction.Value.Invoke(new BlittablePointer<byte>((byte*) address));

            if (bytesFreed > 0)
                Native.VirtualUnlock((IntPtr) newHeader, (UIntPtr) bytesFreed);

            return result;
        }

        [UnmanagedCallersOnly]
        private static unsafe void* FreeFrameStatic(void* address) => _this.FreeFrame(address).Pointer;
    }
}
