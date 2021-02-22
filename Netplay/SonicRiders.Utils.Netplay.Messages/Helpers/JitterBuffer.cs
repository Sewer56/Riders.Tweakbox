﻿using System;
using System.Collections.Generic;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Helpers
{
    /// <summary>
    /// A simple implementation of a jitter buffer, which takes in values and allows for them to be later dequeued at consistent intervals.
    /// </summary>
    public class JitterBuffer<T> where T : struct, ISequenced, IDisposable
    {
        private static int  ModValue     = new T().MaxValue + 1;

        /// <summary>
        /// Holds the current packet for each sequence value.
        /// </summary>
        private StructArrayMap<int, T> _dictionary = new StructArrayMap<int, T>(ModValue);

        /// <summary>
        /// Time spent in the buffer until an item can be dequeued.
        /// </summary>
        public int NumBufferedPackets { get; private set; }

        /// <summary>
        /// The current sequence value being handled.
        /// </summary>
        public int SlidingWindowStart { get; private set; }

        /// <summary>
        /// End of the sliding window.
        /// </summary>
        public int SlidingWindowEnd => GetNextSequenceNo(SlidingWindowStart, Size / 2);

        /// <summary>
        /// Sliding window but extended the other way, used for filtering packets to drop.
        /// </summary>
        public int SlidingWindowBeforeStart => GetLastSequenceNo(SlidingWindowStart, Size / 2);

        /// <summary>
        /// True if the sliding window is currently allowing dequeuing, else false.
        /// </summary>
        public bool IsDeQueueing { get; private set; } = false;

        /// <summary>
        /// Total amount of packets currently stored.
        /// </summary>
        public int PacketCount => _dictionary.Count;

        /// <summary>
        /// The size of the underlying jitter buffer.
        /// </summary>
        public int Size => ModValue;

        /// <summary>
        /// A simple implementation of a jitter buffer, which takes in values and allows for them to be later dequeued at consistent intervals.
        /// </summary>
        /// <param name="numBufferedPackets">Number of buffered packets.</param>
        public JitterBuffer(int numBufferedPackets)
        {
            if (numBufferedPackets > Size / 2)
                throw new ArgumentOutOfRangeException($"Number of buffered packets {numBufferedPackets} is set too high for the sliding window size taken from {nameof(ISequenced)} ({ModValue}). Decrease number of buffered packets or increase window size.");

            NumBufferedPackets = numBufferedPackets;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            var enumerator = _dictionary.Values();
            while (enumerator.MoveNext())
                enumerator.Current.Dispose();

            _dictionary.Clear();
            IsDeQueueing = false;
        }

        /// <summary>
        /// Adds an item to the queue.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <param name="allowPacketDrop">Allows for existing packets in the buffer to be dropped to ensure sender doesn't overwhelm receiver.</param>
        /// <returns>True if the item is queued. False if it is dropped.</returns>
        public bool TryEnqueue(in T value, bool allowPacketDrop = false)
        {
            var sequenceNo = value.SequenceNo;

            // Set sequence value if this is the only message.
            if (PacketCount == 0)
                SlidingWindowStart = value.SequenceNo;

            // Discard items if behind us in the sliding window.
            if (IsInSlidingWindow(value.SequenceNo, SlidingWindowBeforeStart, SlidingWindowStart))
               return false;

            // Check if items ahead of us are not overwhelming the receiver.
            if (allowPacketDrop && IsInSlidingWindow(value.SequenceNo, SlidingWindowStart, SlidingWindowEnd))
            {
                var numSequenceAhead = GetSequenceDifference(value.SequenceNo);
                var readAheadNo      = (numSequenceAhead - NumBufferedPackets);

                // Dequeue necessary number of packets for consumer to catch up.
                while (readAheadNo >= 0)
                {
                    DisposeDequeue();
                    readAheadNo--;
                }
            }

            // Dispose any old value (e.g. received duplicate from network).
            DisposeAtIndex(sequenceNo);
            _dictionary[sequenceNo] = value;

            if (PacketCount >= NumBufferedPackets)
                IsDeQueueing = true;

            return true;
        }

        /// <summary>
        /// Removes an item from the queue.
        /// </summary>
        /// <param name="value">The item.</param>
        public bool TryDequeue(out T value)
        {
            if (IsDeQueueing)
            {
                var result = _dictionary.Remove(IncrementSequenceNo(), out value);
                if (PacketCount <= 0)
                    IsDeQueueing = false;

                return result;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the sequence number that would naturally come after the provided number.
        /// </summary>
        /// <param name="sequenceNo">The current sequence number.</param>
        /// <param name="incrementNumber">Amount by which to increment the sequence number by.</param>
        public int GetNextSequenceNo(int sequenceNo, int incrementNumber = 1) => (sequenceNo + incrementNumber) % ModValue;

        /// <summary>
        /// Gets the sequence number that would naturally come before the provided number.
        /// </summary>
        /// <param name="sequenceNo">The current sequence number.</param>
        /// <param name="decrementNumber">Amount by which to decrement the sequence number by.</param>
        public int GetLastSequenceNo(int sequenceNo, int decrementNumber = 1)
        {
            var result = sequenceNo - decrementNumber;
            if (result >= 0)
                return result;

            return ModValue + result;
        }

        /// <summary>
        /// Increments the sequence number and returns the original number.
        /// </summary>
        public int IncrementSequenceNo()
        {
            var currentNo = SlidingWindowStart;
            SlidingWindowStart = GetNextSequenceNo(SlidingWindowStart);
            return currentNo;
        }

        /// <summary>
        /// Determines if a given sequence number fits the sliding window. Discard packet if it does not.
        /// </summary>
        public bool IsInSlidingWindow(int sequenceNo) => IsInSlidingWindow(sequenceNo, SlidingWindowStart, SlidingWindowEnd);

        /// <summary>
        /// Checks if a given sequence number fits within a sliding window.
        /// </summary>
        /// <param name="sequenceNo">The sequence number.</param>
        /// <param name="startIndex">Starting value of the window.</param>
        /// <param name="endIndex">Ending value of the window.</param>
        public static bool IsInSlidingWindow(int sequenceNo, int startIndex, int endIndex)
        {
            // Check no overflow scenario (most common).
            if (sequenceNo >= startIndex && sequenceNo < endIndex)
                return true;

            // Check for overflow and then possible cases.
            bool overflow = endIndex < startIndex;
            if (overflow)
            {
                if (sequenceNo < endIndex)
                    return true;

                if (sequenceNo >= startIndex && sequenceNo < ModValue)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the number of items the given sequence number is ahead of the current sliding window start.
        /// </summary>
        /// <param name="sequenceNo">The sequence number.</param>
        private int GetSequenceDifference(int sequenceNo)
        {
            var numSequenceAhead  = 0;
            var currentSequenceNo = sequenceNo;
            while (currentSequenceNo != SlidingWindowStart)
            {
                currentSequenceNo = GetLastSequenceNo(currentSequenceNo);
                numSequenceAhead++;
            }

            return numSequenceAhead;
        }

        private void DisposeAtIndex(int index)
        {
            if (_dictionary.TryGetValue(index, out var oldValue))
                oldValue.Dispose();
        }

        private void DisposeDequeue()
        {
            if (TryDequeue(out var oldValue))
                oldValue.Dispose();
        }
    }
}