using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riders.Tweakbox.Controllers.Interfaces;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Various patches for pushing the game a bit more towards the limit.
    /// </summary>
    public class LimitBreakController : IController
    {
        public unsafe LimitBreakController()
        {
            // Unmanaged Heap
            // 3000000 (50MB) -> 4C4B400 (80MB)
            *(int*) 0x527C24 = 0x4C4B400;
            *(int*) 0x527C5E = 0x4C4B400;

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
