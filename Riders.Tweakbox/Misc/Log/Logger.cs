using System.Runtime.CompilerServices;
namespace Riders.Tweakbox.Misc.Log;

/// <summary>
/// Utility class which simplifies logging to a specific category.
/// </summary>
public struct Logger
{
    /// <summary>
    /// The category log data should be sent to.
    /// </summary>
    public LogCategory Category { get; set; }

    public Logger(LogCategory category) => Category = category;

    /// <summary>
    /// Checks if a given category is enabled.
    /// </summary>
    public bool IsEnabled(ListenerType listenerType = ListenerType.Any) => Log.IsEnabled(Category, listenerType);

    /// <summary>
    /// Logs a given piece of text onto a new line.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void WriteLine(string text) => Log.WriteLine(text, Category);

    /// <summary>
    /// Logs a given piece of text onto a new line.
    /// </summary>
    /// <param name="handler">The text to write.</param>
    public void WriteLine([InterpolatedStringHandlerArgument("")] LoggerInterpolatedStringHandler handler) => Log.WriteLine(handler.ToString(), Category);

    /// <summary>
    /// Logs a given piece of text onto the existing line.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void Write(string text) => Log.Write(text, Category);

    /// <summary>
    /// Logs a given piece of text onto the existing line.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void Write(LoggerInterpolatedStringHandler handler) => Log.Write(handler.ToString(), Category);
}
