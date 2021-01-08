using System;
using System.Diagnostics;
using Sewer56.Imgui.Shell.Structures;

namespace Sewer56.Imgui.Utilities
{
    public class ShellTraceListener : TraceListener
    {
        /// <summary>
        /// The timeout before the item disappears from the screen.
        /// </summary>
        public int Timeout { get; set; } = 5000;

        public override void Write(string message)
        {
            var time = DateTime.UtcNow;
            Shell.Shell.Log(new LogItem($"[{time.Second:00}.{time.Millisecond:000}] {message}", Timeout));
        }

        public override void WriteLine(string message)
        {
            var time = DateTime.UtcNow;
            Shell.Shell.Log(new LogItem($"[{time.Second:00}.{time.Millisecond:000}] {message}", Timeout));
        }
    }
}
