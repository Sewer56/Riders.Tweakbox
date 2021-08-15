using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace Riders.Tweakbox.Misc;

public static class ActionWrappers
{
    /// <summary>
    /// Waits until a given condition is met.
    /// </summary>
    /// <param name="function">Waits until this condition returns true or the timeout expires.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    /// <param name="token">Token that allows for cancellation of the task.</param>
    /// <returns>True if the condition is triggered, else false.</returns>
    public static bool TryWaitUntil(Func<bool> function, int timeout, int sleepTime = 1, CancellationToken token = default)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        while (watch.ElapsedMilliseconds < timeout)
        {
            if (token.IsCancellationRequested)
                return false;

            var flag = function();
            if (flag)
                return true;

            Thread.Sleep(sleepTime);
        }

        return false;
    }

    /// <summary>
    /// Waits until a given condition is met.
    /// </summary>
    /// <param name="function">Waits until this condition returns true or the timeout expires.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
    /// <param name="token">Token that allows for cancellation of the task.</param>
    /// <returns>True if the condition is triggered, else false.</returns>
    public static async Task<bool> TryWaitUntilAsync(Func<bool> function, int timeout, int sleepTime = 1, CancellationToken token = default)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        while (watch.ElapsedMilliseconds < timeout)
        {
            if (token.IsCancellationRequested)
                return false;

            var flag = function();
            if (flag)
                return true;

            await Task.Delay(sleepTime, token);
        }

        return false;
    }
}
