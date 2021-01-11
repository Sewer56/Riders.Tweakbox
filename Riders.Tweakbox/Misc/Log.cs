using System;
using System.Diagnostics;
using EnumsNET;

namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Different categories that can be used for logging.
    /// </summary>
    [Flags]
    public enum LogCategory
    {
        Default = 1,
        Memory  = 2,
        Race    = 4,
        Menu    = 8,
        Random  = 16,
        Ntp     = 32,
        Socket  = 64,
    }

    /// <summary>
    /// Provides logging support for Tweakbox
    /// </summary>
    public static class Log
    {
        static Log()
        {
            foreach (var value in Enums.GetValues<LogCategory>())
            {
                EnabledCategories |= value;
            }

            // Disable spammy logs by default.
            EnabledCategories &= ~LogCategory.Random;
        }

        /// <summary>
        /// Declares whether each log type is declared or not.
        /// </summary>
        public static LogCategory EnabledCategories = LogCategory.Default;

        /// <summary>
        /// Checks if a given category is enabled.
        /// </summary>
        public static bool IsEnabled(LogCategory category) => EnabledCategories.HasAnyFlags(category);

        /// <summary>
        /// Logs a given piece of text onto a new line.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="category">Category of the text.</param>
        public static void WriteLine(string text, LogCategory category)
        {
            if (IsEnabled(category))
                Trace.WriteLine(text);
        }

        /// <summary>
        /// Logs a given piece of text.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="category">Category of the text.</param>
        public static void Write(string text, LogCategory category)
        {
            if (IsEnabled(category))
                Trace.Write(text);
        }
    }
}
