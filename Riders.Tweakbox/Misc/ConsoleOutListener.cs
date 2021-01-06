using System;
using System.Diagnostics;
using Reloaded.Mod.Interfaces;

namespace Riders.Tweakbox.Misc
{
    internal class ConsoleOutListener : TraceListener
    {
        private ILogger _logger;

        /// <inheritdoc />
        public ConsoleOutListener(ILogger logger)
        {
            _logger = logger;
        }

        public override void Write(string message)     => _logger.WriteAsync(message);
        public override void WriteLine(string message) => _logger.WriteLineAsync($"[{DateTime.UtcNow.Second}.{DateTime.UtcNow.Millisecond}] {message}");
    }
}
