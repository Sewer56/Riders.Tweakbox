using System;

namespace Sewer56.Imgui.Shell.Structures
{
    /// <summary>
    /// Encapsulates an individual log item.
    /// </summary>
    public class LogItem
    {
        public string Text { get; set; }
        public int Timeout { get; set; }
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Contains an individual item to be displayed by the log.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="timeout">The timeout in millseconds.</param>
        public LogItem(string text, int timeout = 5000)
        {
            Text = text;
            Timeout = timeout;
            CreationDate = DateTime.UtcNow;
        }

        /// <summary>
        /// True if the log has expired, else false.
        /// </summary>
        public bool HasExpired()
        {
            var delta = DateTime.UtcNow - CreationDate;
            return delta.TotalMilliseconds > Timeout;
        }
    }
}
