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
            Shell.Shell.Log(new LogItem(message, Timeout));
        }

        public override void WriteLine(string message)
        {
            Shell.Shell.Log(new LogItem(message, Timeout));
        }
    }
}
