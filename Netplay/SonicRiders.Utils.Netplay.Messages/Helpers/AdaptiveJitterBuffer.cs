﻿using System;
using System.Diagnostics;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Netplay.Messages.Misc.Interfaces;
namespace Riders.Netplay.Messages.Helpers;

public class AdaptiveJitterBufferConstants
{
    public static float JitterRampUpPercentile = 0.95f;
    public static float JitterRampDownPercentile = 1.00f;
    public static int PacketSendRate = 60;
}

/// <summary>
/// Basic implementation of an adaptive jitter buffer which scales the default buffer's number of queued frames as necessary.
/// Advantages:
/// - Fast to roll-forward.
/// - Potentially better latency.
///
/// Disadvantages:
/// - Jitter on unexpected ping spike.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AdaptiveJitterBuffer<T> : IJitterBuffer<T> where T : struct, ISequenced, IDisposable
{
    /// <summary>
    /// Underlying basic jitter buffer.
    /// </summary>
    public JitterBuffer<T> Buffer;

    /// <summary>
    /// Calculatoir for the default buffer.
    /// </summary>
    private JitterCalculator _calculator;

    /// <summary>
    /// Internal jitter buffer used for reducing current amount of queued frames on existing buffers.
    /// </summary>
    internal JitterBuffer<SequenceNumberCopy<T>>[] _jitterBufferRollforward;

    /// <summary>
    /// Internal jitter buffer used for reducing current amount of queued frames on existing buffers.
    /// </summary>
    internal JitterCalculator[] _jitterCalculatorRollforward;

    public AdaptiveJitterBuffer(int defaultBufferSize = 3, int calculatorAmountFrames = 120, int rollForwardAmount = 3)
    {
        Buffer = new JitterBuffer<T>(defaultBufferSize, true);
        _calculator = new JitterCalculator(calculatorAmountFrames);
        _jitterCalculatorRollforward = new JitterCalculator[rollForwardAmount];
        _jitterBufferRollforward = new JitterBuffer<SequenceNumberCopy<T>>[rollForwardAmount];
        for (int x = 0; x < _jitterCalculatorRollforward.Length; x++)
        {
            _jitterCalculatorRollforward[x] = new JitterCalculator(calculatorAmountFrames);
            _jitterBufferRollforward[x] = new JitterBuffer<SequenceNumberCopy<T>>(defaultBufferSize - (x + 1), true);
        }
    }

    /// <inheritdoc />
    public JitterBufferType GetBufferType() => JitterBufferType.Adaptive;

    public void Clear()
    {
        Buffer.Clear();
        _calculator.Reset();

        for (int x = 0; x < _jitterBufferRollforward.Length; x++)
            _jitterBufferRollforward[x].Clear();

        for (int x = 0; x < _jitterCalculatorRollforward.Length; x++)
            _jitterCalculatorRollforward[x].Reset();
    }

    /// <summary>
    /// Tries to add a packet to the adaptive jitter buffer queue.
    /// </summary>
    /// <param name="packet"></param>
    public bool TryEnqueue(in T packet)
    {
        bool result = Buffer.TryEnqueue(packet);
        var sequenceNumber = SequenceNumberCopy<T>.Create(packet);
        for (int x = 0; x < _jitterBufferRollforward.Length; x++)
            _jitterBufferRollforward[x].TryEnqueue(sequenceNumber);

        return result;
    }

    /// <summary>
    /// Tries to dequeue a packet from the adaptive buffer.
    /// </summary>
    public bool TryDequeue(int playerIndex, out T packet)
    {
        bool dequeue = TryDequeueInternal(out packet, out bool isNowEmpty);
        UpdateBuffer(playerIndex);
        if (isNowEmpty)
            SetBufferSize(Buffer.BufferSize + 1);

        return dequeue;
    }

    /// <summary>
    /// Sets the size for the new buffer.
    /// </summary>
    /// <param name="newSize">New buffer size.</param>
    public void SetBufferSize(int newSize)
    {
        if (newSize != Buffer.BufferSize)
        {
            Buffer.SetBufferSize(newSize);
            for (int x = 0; x < _jitterBufferRollforward.Length; x++)
            {
                _jitterBufferRollforward[x].SetBufferSize(Buffer.BufferSize - (x + 1));
                _jitterCalculatorRollforward[x].Reset();
            }
        }
    }

    /// <summary>
    /// Updates the jitter buffer values if necessary.
    /// </summary>
    private void UpdateBuffer(int playerIndex)
    {
        // Calculate if buffer should be increased.
        if (_calculator.TryCalculateJitter(AdaptiveJitterBufferConstants.JitterRampUpPercentile, out var maxJitter))
        {
            int extraFrames = (int)(maxJitter / (1000f / AdaptiveJitterBufferConstants.PacketSendRate));
            Debug.WriteLine($"Jitter P[{playerIndex}]. Max: {maxJitter}, Extra Frames: {extraFrames}");

            // If the buffer size is increasing, the jitter buffer itself will handle this case (if queued packets ever reaches 0)
            if (extraFrames > 0)
                SetBufferSize(Buffer.BufferSize + extraFrames);
        }

        // Check if buffer should be decreased.
        for (int x = _jitterBufferRollforward.Length - 1; x >= 0; x--)
        {
            var rollForwardBuffer = _jitterBufferRollforward[x];
            var rollForwardCalculator = _jitterCalculatorRollforward[x];
            if (rollForwardCalculator.TryCalculateJitter(AdaptiveJitterBufferConstants.JitterRampDownPercentile, out maxJitter))
            {
                int extraBufferedPackets = (int)(maxJitter / (1000f / AdaptiveJitterBufferConstants.PacketSendRate));
                if (extraBufferedPackets > 0)
                    continue;

                if (rollForwardBuffer.BufferSize != Buffer.BufferSize && rollForwardBuffer.BufferSize > JitterBuffer<T>.MinBufferSize)
                {
                    Debug.WriteLine($"Reduce Buffer Jitter P[{playerIndex}].");
                    SetBufferSize(rollForwardBuffer.BufferSize);
                    return;
                }
            }
        }
    }

    private bool TryDequeueInternal(out T packet, out bool isNowEmpty)
    {
        // Do sampling for roll forward buffers.
        for (int x = 0; x < _jitterBufferRollforward.Length; x++)
        {
            if (_jitterBufferRollforward[x].TryDequeue(out var sequence))
                _jitterCalculatorRollforward[x].Sample();
        }

        // Try dequeue.
        if (Buffer.TryDequeue(out packet, out isNowEmpty))
        {
            _calculator.Sample();
            return true;
        }

        return false;
    }
}
