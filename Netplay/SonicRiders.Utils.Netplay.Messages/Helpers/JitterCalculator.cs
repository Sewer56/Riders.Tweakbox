using System;
using Reloaded.WPF.Animations.FrameLimiter;
using StructLinq;

namespace Riders.Netplay.Messages.Helpers
{
    public class JitterCalculator
    {
        private CircularBuffer<double> _receiveTimes;
        private DateTime? _lastInsertTime;

        private int Size => _receiveTimes.Capacity;
        private bool IsFull => _currentNumSamples >= _receiveTimes.Capacity;
        private int _currentNumSamples;

        /// <summary>
        /// Calculates jitter for a specified amount of time in frames.
        /// </summary>
        /// <param name="frames">Number of frames to sample.</param>
        public JitterCalculator(int frames = 120)
        {
            _receiveTimes      = new CircularBuffer<double>(frames);
            _currentNumSamples = 0;
        }

        /// <summary>
        /// Samples a value for the jitter calculator.
        /// </summary>
        public void Sample()
        {
            var timeSinceLast = DateTime.UtcNow - _lastInsertTime.GetValueOrDefault(DateTime.UtcNow);
            _lastInsertTime = DateTime.UtcNow;
            _receiveTimes.PushBack(timeSinceLast.TotalMilliseconds);
            _currentNumSamples++;
        }

        /// <summary>
        /// Calculates the amount of jitter that occurred during this time period.
        /// </summary>
        /// <param name="percentile">The percentile between 0 and 1 which to use for calculating jitter.</param>
        /// <param name="maxJitter">Maximum amount of jitter that occurred.</param>
        /// <returns>False if not enough data has yet been sampled since last calculation, otherwise true.</returns>
        public bool TryCalculateJitter(float percentile, out double maxJitter)
        {
            if (IsFull)
            {
                _currentNumSamples = 0;
                int elementToSample = (int) (percentile * (Size - 1));

                // Average time between successful receives.
                var receiveTimes = _receiveTimes.ToStructEnumerable().Take(Size, x => x).ToArray(x => x);
                double totalDifference = 0;
                maxJitter   = 0;

                for (int x = 0; x < receiveTimes.Length - 1; x++)
                    receiveTimes[x] = Math.Abs(receiveTimes[x] - receiveTimes[x + 1]);

                maxJitter = receiveTimes.ToStructEnumerable().Order(x => x).ElementAt(elementToSample, x => x);
                return true;
            }

            maxJitter = 0;
            return false;
        }

        /// <summary>
        /// Resets the current number of taken samples.
        /// </summary>
        public void Reset() => _currentNumSamples = 0;
    }
}