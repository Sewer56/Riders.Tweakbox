using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Utilities = Sewer56.SonicRiders.Utilities;
using Windows.Win32;
using Riders.Tweakbox.Configs;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Functions;
namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Various patches for pushing the game a bit more towards the limit.
/// </summary>
public class LimitBreakController : IController
{
    public bool IsLargeAddressAware { get; private set; }

    public unsafe LimitBreakController(TweakboxConfig config)
    {
        var characteristics = (short*)0x40013E;
        IsLargeAddressAware = (*characteristics & 0x20) != 0;

        // Unmanaged Heap
        // 3000000 (50MB) -> 7A120000 (???)
        int heapSize = (int)config.Data.MemoryLimit;
        *(int*)0x527C24 = heapSize;
        *(int*)0x527C5E = heapSize;

        // Extend Task Heap
        // 1024 -> 40960 Total Tasks
        // 200 -> 8000 Task Heap 1
        // 800 -> 32000 Task Heap 2
        *(int*)0x4186FC = 40960;
        *(int*)0x4186F7 = 8000;
        *(int*)0x4186CF = 32000;

        // 8192 Collision Object Limit (from 600)
        *(int*)0x441954 = 0x420000; // Memory Allocated
        *(int*)0x4419AD = 0x2000; // Init Loop Iterations
        *(int*)0x441990 = 0x2000; // Init Loop Iterations
    }
}
