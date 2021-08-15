using System;
using System.Diagnostics;
using System.Linq;
using Reloaded.WPF.Animations.FrameLimiter;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Netplay.Messages.Misc.Interfaces;
namespace Riders.Netplay.Messages.Helpers;

/// <summary>
/// Represents a "hybrid" jitter buffer sacrifices a bit of latency for smoothness, slowly rolling forward over time as needed.
/// Advantages:
/// - Smooth
/// Disadvantages:
/// - May be a frame or two behind adaptive buffer.
/// </summary>
public class HybridJitterBuffer<T> : IJitterBuffer<T> where T : struct, ISequenced, IDisposable
{
    /// <summary>
    /// Jitter buffer for storing current movements.
    /// </summary>
    public JitterBuffer<T> Buffer;

    /// <summary>
    /// Calculates the remaining number of buffered packets after dequeuing for each sampled period.
    /// </summary>
    public CircularBuffer<byte> _remainingNumPacketsArray;
    private int _currentNumSamples;
    private const int MinBufferSize = 1;

    public HybridJitterBuffer(int defaultBufferSize = 3, int calculatorAmountFrames = 120)
    {
        Buffer = new JitterBuffer<T>(defaultBufferSize, false);
        _remainingNumPacketsArray = new CircularBuffer<byte>(calculatorAmountFrames);
    }

    /// <inheritdoc />
    public JitterBufferType GetBufferType() => JitterBufferType.Hybrid;

    /// <inheritdoc />
    public void Clear()
    {
        Buffer.Clear();
        _currentNumSamples = 0;
    }

    /// <inheritdoc />
    public bool TryEnqueue(in T packet) => Buffer.TryEnqueue(packet);

    /// <inheritdoc />
    public bool TryDequeue(int playerIndex, out T packet)
    {
        bool result = TryDequeueInternal(out packet);
        if (_currentNumSamples > _remainingNumPacketsArray.Capacity)
        {
            var minimumToDequeue = _remainingNumPacketsArray.Min() - MinBufferSize;

#if DEBUG
            if (minimumToDequeue > 0)
                Debug.WriteLine($"P[{playerIndex}] Reducing Hybrid Buffer Jitter by {minimumToDequeue}");
#endif

            for (int x = 0; x < minimumToDequeue; x++)
                result = Buffer.TryDequeue(out packet);

            _currentNumSamples = 0;
        }

        return result;
    }

    /// <inheritdoc />
    public void SetBufferSize(int newSize) => Buffer.SetBufferSize(newSize);

    private bool TryDequeueInternal(out T packet)
    {
        bool result = Buffer.TryDequeue(out packet);
        _remainingNumPacketsArray.PushBack((byte)Buffer.GetNumPacketsInWindow());
        _currentNumSamples++;
        return result;
    }
}
