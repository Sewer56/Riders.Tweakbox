using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.Functions;
using Utilities = Sewer56.SonicRiders.Utilities;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Riders.Tweakbox.Configs;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Various patches for pushing the game a bit more towards the limit.
    /// </summary>
    public class LimitBreakController : IController
    {
        public bool IsLargeAddressAware { get; private set; }

        private IHook<Heap.FreeFnPtr> _freeFrameHook;
        private static LimitBreakController _this;
        private SYSTEM_INFO _info;

        public unsafe LimitBreakController(TweakboxConfig config)
        {
            _this = this;

            // 2GB Heap
            var characteristics = (short*)0x40013E;
            IsLargeAddressAware = (*characteristics & 0x20) != 0;

            // Unmanaged Heap
            // 3000000 (50MB) -> 7A120000 (???)
            int heapSize = (int) config.Data.MemoryLimit;
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

            _freeFrameHook = Heap.FreeFrame.HookAs<Heap.FreeFnPtr>(typeof(LimitBreakController), nameof(FreeFrameStatic)).Activate();
            PInvoke.GetSystemInfo(out _info);
        }

        private unsafe BlittablePointer<byte> FreeFrame(void* address)
        {
            uint currentHeader = *(uint*)0x017B8DA8;
            uint newHeader     = (uint) address;
            int bytesFreed     = (int) (currentHeader - newHeader); 

            var result = _freeFrameHook.OriginalFunction.Value.Invoke(new BlittablePointer<byte>((byte*) address));

            var freeStart = Utilities.RoundUp((int) newHeader, (int)_info.dwPageSize);
            int bytesRoundLess = (int) (freeStart - newHeader);
            var freeSize  = RoundDown(bytesFreed - bytesRoundLess, (int) _info.dwPageSize);

            if (freeSize > 0)
                Native.VirtualUnlock((IntPtr) freeStart, (UIntPtr) freeSize);

            return result;
        }

        public static int RoundDown(int number, int multiple)
        {
            if (multiple == 0)
                return number;

            return number / multiple * multiple;
        }

        [UnmanagedCallersOnly]
        private static unsafe void* FreeFrameStatic(void* address) => _this.FreeFrame(address).Pointer;
    }
}
