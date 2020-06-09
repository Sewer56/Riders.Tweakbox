using System;
using System.Collections.Generic;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Tweakbox.Components.Netplay;

namespace Riders.Tweakbox.Misc
{
    public static class Extensions
    {
        public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*) 0x0;
    }
}
