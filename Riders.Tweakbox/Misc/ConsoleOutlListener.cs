using System;
using System.Diagnostics;

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
            Console.WriteLine($"[{DateTime.UtcNow.Second}.{DateTime.UtcNow.Millisecond}] {message}");
        }
    }
}
