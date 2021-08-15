using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Functions;
using Microsoft.Windows.Sdk;
using static Riders.Tweakbox.Misc.Log.Log;
using Riders.Netplay.Messages.Misc;
using System;

namespace Riders.Tweakbox.Controllers;

public class HeapController : IController
{
    /// <summary>
    /// Pointer to the last allocated memory block.
    /// </summary>
    public unsafe MallocResult* FirstAllocResult { get; private set; }

    private static HeapController _instance;
    private IHook<Heap.AllocFnPtr> _mallocHook;
    private IHook<Heap.AllocFnPtr> _callocHook;
    private IHook<Heap.FreeFnPtr> _freeHook;
    private IHook<Heap.FreeFrameFnPtr> _freeFrameHook;
    private Logger _heapLogger = new Logger(LogCategory.Heap);
    private SYSTEM_INFO _info;

    public HeapController(IReloadedHooks hooks)
    {
        _instance = this;
        PInvoke.GetSystemInfo(out _info);

        _mallocHook = Heap.Malloc.HookAs<Heap.AllocFnPtr>(typeof(HeapController), nameof(MallocImplStatic)).Activate();
        _callocHook = Heap.Calloc.HookAs<Heap.AllocFnPtr>(typeof(HeapController), nameof(CallocImplStatic)).Activate();
        _freeHook = Heap.Free.HookAs<Heap.FreeFnPtr>(typeof(HeapController), nameof(FreeImplStatic)).Activate();
        _freeFrameHook = Heap.FreeFrame.HookAs<Heap.FreeFrameFnPtr>(typeof(HeapController), nameof(FreeFrameImplStatic)).Activate();
    }

    private unsafe int FreeFrameImpl(MallocResult* address)
    {
        /*
            In our FreeFrame hook, we evict the freed memory from
            physical RAM; allowing it to be swapped out if necessary.

            This is an optimisation in the case where e.g. 
            - A large custom map is being unloaded.
            - Then a regular map is being loaded.

            Normally the remainder of the old map would still be in RAM
            in the unused region of the buffer; but since that region
            will be unused, we tell the OS it's free to put something
            else in that physical RAM area.
         */

        var currentHeader = *Heap.FrameHeadFront;
        var newHeader = address;
        int bytesFreed = (int)(currentHeader - newHeader);

        _heapLogger.WriteLine($"FreeFrame: {(long)address:X}");
        var result = _freeFrameHook.OriginalFunction.Value.Invoke(address);

        var freeStart = Utilities.RoundUp((long)newHeader, _info.dwPageSize);
        int bytesRoundLess = (int)(freeStart - (long)newHeader);
        var freeSize = Utilities.RoundDown(bytesFreed - bytesRoundLess, (int)_info.dwPageSize);

        if (freeSize > 0)
            Native.VirtualUnlock((IntPtr)freeStart, (UIntPtr)freeSize);

        return result;
    }

    private unsafe MallocResult* FreeImpl(MallocResult* address)
    {
        _heapLogger.WriteLine($"Free: {(long)address:X}");
        var result = _freeHook.OriginalFunction.Value.Invoke(address).Pointer;

        // Erase the contents of the allocation header.
        // This is necessary for our heap walker in the Heap Debug window.
        var header = result->GetHeader(result);
        header->Base = (MallocResult*)0;
        header->AllocationSize = 0;

        return result;
    }

    private unsafe MallocResult* CallocImpl(int alignment, int size)
    {
        var result = _callocHook.OriginalFunction.Value.Invoke(alignment, size).Pointer;
        var header = result->GetHeader(result);
        if (header->Base == *Heap.FirstHeaderFront)
            FirstAllocResult = result;

        _heapLogger.WriteLine($"Calloc: {(long)result:X} | Alignment {alignment}, Size {size}");
        _heapLogger.WriteLine($"Header [{(long)header:X}] | Base: {(long)header->Base:X}, Size: {header->AllocationSize}");
        return result;
    }

    private unsafe MallocResult* MallocImpl(int alignment, int size)
    {
        var result = _mallocHook.OriginalFunction.Value.Invoke(alignment, size).Pointer;
        var header = result->GetHeader(result);
        if (header->Base == *Heap.FirstHeaderFront)
            FirstAllocResult = result;

        _heapLogger.WriteLine($"Malloc: {(long)result:X} | Alignment {alignment}, Size {size}");
        _heapLogger.WriteLine($"Header [{(long)header:X}] | Base: {(long)header->Base:X}, Size: {header->AllocationSize}");
        return result;
    }

    #region Static Entry Points
    [UnmanagedCallersOnly]
    private static unsafe MallocResult* MallocImplStatic(int alignment, int size) => _instance.MallocImpl(alignment, size);

    [UnmanagedCallersOnly]
    private static unsafe MallocResult* CallocImplStatic(int alignment, int size) => _instance.CallocImpl(alignment, size);

    [UnmanagedCallersOnly]
    private static unsafe MallocResult* FreeImplStatic(MallocResult* address) => _instance.FreeImpl(address);

    [UnmanagedCallersOnly]
    private static unsafe int FreeFrameImplStatic(MallocResult* address) => _instance.FreeFrameImpl(address);
    #endregion
}
