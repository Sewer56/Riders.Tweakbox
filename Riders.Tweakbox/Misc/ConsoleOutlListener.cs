using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Riders.Tweakbox.Misc
{
    internal class ConsoleOutlListener : TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow.Millisecond}] {message}");
        }
    }
}
