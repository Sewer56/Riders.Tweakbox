using System;
using System.Diagnostics;
using Reloaded.Mod.Interfaces;
namespace Riders.Tweakbox.Misc;

internal class ConsoleOutListener : TraceListener
{
    private ILogger _logger;

    /// <inheritdoc />
    public ConsoleOutListener(ILogger logger)
    {
        _logger = logger;
    }

    public override void Write(string message) => _logger.WriteAsync(message);
    public override void WriteLine(string message)
    {
        var time = DateTime.UtcNow;
        _logger.WriteLineAsync($"[{time.Minute}:{time.Second:00}.{time.Millisecond:000}] {message}");
    }
}
