using System;
using System.Diagnostics;
using System.Threading;

namespace Riders.Tweakbox.Misc
{
    public static class ActionWrappers
    {
        /// <summary>
        /// A wrapper for try/catch that swallows exceptions.
        /// </summary>
        public static void TryCatch(Action action)
        {
            try { action(); }
            catch (Exception) { /* ignored */ }
        }

        /// <param name="condition">Stops sleeping if this condition returns true.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        public static void SleepOnConditionWithTimeout(Func<bool> condition, int timeout, int sleepTime)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (watch.ElapsedMilliseconds < timeout && !condition())
            {
                Thread.Sleep(sleepTime);
            }
        }

        /// <param name="getValue">Function that retrieves the value.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        /// <exception cref="Exception">Timeout expired.</exception>
        public static T TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime = 1, CancellationToken token = default)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool valueSet = false;
            T value = default;

            while (watch.ElapsedMilliseconds < timeout)
            {
                if (token.IsCancellationRequested)
                    return value;

                try
                {
                    value = getValue();
                    valueSet = true;
                    break;
                }
                catch (Exception) { /* Ignored */ }

                Thread.Sleep(sleepTime);
            }

            if (valueSet == false)
                throw new Exception($"Timeout limit {timeout} exceeded.");

            return value;
        }

        /// <summary>
        /// Attempts to obtain a value while either the timeout has not expired or the <see cref="whileFunction"/> returns
        /// true.
        /// </summary>
        /// <param name="getValue">Function that retrieves the value.</param>
        /// <param name="whileFunction">Keep trying while this condition is true.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        /// <exception cref="Exception">Timeout expired.</exception>
        public static T TryGetValueWhile<T>(Func<T> getValue, Func<bool> whileFunction, int timeout, int sleepTime = 1, CancellationToken token = default)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool valueSet = false;
            T value = default;

            while (watch.ElapsedMilliseconds < timeout || whileFunction())
            {
                if (token.IsCancellationRequested)
                    return value;

                try
                {
                    value = getValue();
                    valueSet = true;
                    break;
                }
                catch (Exception) { /* Ignored */ }

                Thread.Sleep(sleepTime);
            }

            if (valueSet == false)
                throw new Exception($"Timeout limit {timeout} exceeded.");

            return value;
        }

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
    }
}
