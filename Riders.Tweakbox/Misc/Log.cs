using System;
using System.Diagnostics;
using EnumsNET;

namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Different categories that can be used for logging.
    /// </summary>
    [Flags]
    public enum LogCategory : short
    {
        Default = 1 << 0,
        Memory  = 1 << 1,
        Race    = 1 << 2,
        Menu    = 1 << 3,
        Random  = 1 << 4,
        PlayerEvent = 1 << 5,
        Socket  = 1 << 6,
        RandomSeed = 1 << 7,
        LapSync = 1 << 8,
        JitterCalc = 1 << 9,
    }

    /// <summary>
    /// Provides logging support for Tweakbox.
    /// </summary>
    public static class Log
    {
        public static TraceListener ConsoleListener;
        public static TraceListener HudListener;

        /// <summary>
        /// Default enabled logging categories
        /// </summary>
        public const LogCategory DefaultHudCategories = LogCategory.Socket;

        /// <summary>
        /// Default enabled logging categories
        /// </summary>
        public const LogCategory DefaultConsoleCategories = LogCategory.Default | LogCategory.Memory |
                                                     LogCategory.Race | LogCategory.Menu |
                                                     LogCategory.PlayerEvent | LogCategory.Socket |
                                                     LogCategory.Random | LogCategory.JitterCalc;

        /// <summary>
        /// Declares whether each log type is declared or not.
        /// </summary>
        public static LogCategory HudCategories = DefaultHudCategories;

        /// <summary>
        /// Declares whether each log type is declared or not.
        /// </summary>
        public static LogCategory ConsoleCategories = DefaultConsoleCategories;
        
        /// <summary>
        /// Checks if a given category is enabled.
        /// </summary>
        public static bool IsEnabled(ListenerType listenerType, LogCategory category)
        {
            switch (listenerType)
            {
                case ListenerType.Hud: return HudCategories.HasAnyFlags(category);
                case ListenerType.Console: return ConsoleCategories.HasAnyFlags(category);
            }

            return false;
        }

        /// <summary>
        /// Logs a given piece of text onto a new line.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="category">Category of the text.</param>
        public static void WriteLine(string text, LogCategory category = LogCategory.Default)
        {
            if (IsEnabled(ListenerType.Hud, category))
                HudListener?.WriteLine(text);

            if (IsEnabled(ListenerType.Console, category))
                ConsoleListener?.WriteLine(text);
        }

        /// <summary>
        /// Logs a given piece of text.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="category">Category of the text.</param>
        public static void Write(string text, LogCategory category = LogCategory.Default)
        {
            if (IsEnabled(ListenerType.Hud, category))
                HudListener?.Write(text);

            if (IsEnabled(ListenerType.Console, category))
                ConsoleListener?.Write(text);
        }

        public enum ListenerType
        {
            Hud,
            Console
        }
    }
}
