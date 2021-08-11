using System;
using DotNext;

namespace Riders.Netplay.Messages.Helpers
{
    /// <summary>
    /// An implementation of a sliding buffer which allows you to view the last N sampled
    /// values as a continuous array.
    ///
    /// 1. A buffer with Capacity 5 and Size 3 starts with:
    /// [[y]xxxx]
    ///
    /// 2. And would then expand to:
    /// [[yyy]xx]
    ///
    /// 3. And slide until:
    /// [xx[yyy]]
    ///
    /// 4. Once it overflows, the last `Size - 1` elements will be copied and the buffer will revert to state 2.:
    /// [[yyy]xx]
    /// </summary>
    public class SlidingBuffer<T>
    {
        /// <summary>
        /// Gets the position of the head/back of the buffer.
        /// </summary>
        public int BackPtr { get; private set; }

        /// <summary>
        /// The capacity of the internal buffer used to store the sliding buffer.
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// True if the buffer is empty, else false.
        /// </summary>
        public bool IsEmpty => Size == 0;

        /// <summary>
        /// Returns the amount of elements in the buffer.
        /// </summary>
        public int Size => BackPtr < _windowSize ? (BackPtr + 1) : _windowSize;

        /// <summary>
        /// Gets the position of the tail/front of the buffer.
        /// </summary>
        public int FrontPtr
        {
            get
            {
                var front = (BackPtr - (_windowSize - 1));
                return front < 0 ? 0 : front;
            }
        }

        private T[] _buffer;

        // Size of window in buffer.
        private int _windowSize;
        
        /// <summary>
        /// Constructs a sliding buffer given a desired window size.
        /// </summary>
        /// <param name="windowSize">The window size.</param>
        public SlidingBuffer(int windowSize) : this(windowSize, windowSize * 4) { }

        /// <summary>
        /// Constructs a sliding buffer given a desired window size and capacity.
        /// </summary>
        /// <param name="windowSize">The window size.</param>
        /// <param name="bufferSize">The capacity of the internal buffer. Higher capacity means less copies.</param>
        /// <exception cref="ArgumentException">Window size is greater than the buffer size/</exception>
        public SlidingBuffer(int windowSize, int bufferSize)
        {
            if (windowSize > bufferSize)
                throw new ArgumentException("Window size cannot be greater than the buffer size.");

            _buffer = new T[bufferSize];
            _windowSize = windowSize;
            BackPtr = -1;
        }

        /// <summary>
        /// Returns a span representing the elements of the current window.
        /// </summary>
        public Span<T> GetWindow() => _buffer.AsSpan().Slice(FrontPtr, Size);

        /// <summary>
        /// Gets a reference to the last element of the array.
        /// </summary>
        public ref T Back() => ref _buffer[BackPtr];

        /// <summary>
        /// Gets a reference to the first element of the array.
        /// </summary>
        public ref T Front() => ref _buffer[FrontPtr];

        /// <summary>
        /// Pushes a value back to the sliding buffer.
        /// </summary>
        /// <param name="value">The value.</param>
        public void PushBack(in T value)
        {
            Increment();
            _buffer[BackPtr] = value;
        }

        /// <summary>
        /// Pushes the head of the buffer to the next position.
        /// If the buffer goes beyond the end of the capacity,
        /// copies the elements from the end to the beginning of the array.
        /// </summary>
        public void Increment()
        {
            BackPtr++;
            if (BackPtr < Capacity) 
                return;

            // Get last N-1 elements.
            var lastElements = _buffer.AsSpan().Slice(FrontPtr);

            // Copy to front of array.
            lastElements.CopyTo(_buffer);

            // Set new head.
            BackPtr = _windowSize - 1;
        }
    }
}
