using System;
using Riders.Netplay.Messages.Helpers;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class SlidingBufferTests
    {
        [Fact]
        public void OnlyAllowsValidWindowSize()
        {
            Assert.Throws<ArgumentException>(() => new SlidingBuffer<float>(windowSize: 2, bufferSize: 1));
            Assert.Throws<ArgumentException>(() => new SlidingBuffer<float>(windowSize: 3, bufferSize: 1));

            // No Throw
            Assert.NotNull(new SlidingBuffer<float>(windowSize: 2, bufferSize: 2));
            Assert.NotNull(new SlidingBuffer<float>(windowSize: 2, bufferSize: 3));
        }

        [Fact]
        public void LoopsOnIncrement()
        {
            var slidingBuffer = new SlidingBuffer<float>(2, 5);

            Assert.Equal(-1, PostIncrement());

            // Expand
            Assert.Equal(0, PostIncrement());
            Assert.Equal(1, PostIncrement());
            Assert.Equal(2, PostIncrement());
            Assert.Equal(3, PostIncrement());
            Assert.Equal(4, PostIncrement());

            // Loop back to front.
            Assert.Equal(1, PostIncrement());

            int PostIncrement()
            {
                var result = slidingBuffer.BackPtr;
                slidingBuffer.Increment();
                return result;
            }
        }

        [Fact]
        public void PushBack()
        {
            var slidingBuffer = new SlidingBuffer<float>(2, 5);

            // Fill Initial Buffer
            slidingBuffer.PushBack(1f);
            slidingBuffer.PushBack(2f);

            Assert.Equal(1f, slidingBuffer.Front());
            Assert.Equal(2f, slidingBuffer.Back());

            // Test Slide 1
            slidingBuffer.PushBack(3f);
            Assert.Equal(2f, slidingBuffer.Front());
            Assert.Equal(3f, slidingBuffer.Back());

            // Test Slide 2
            slidingBuffer.PushBack(4f);
            Assert.Equal(3f, slidingBuffer.Front());
            Assert.Equal(4f, slidingBuffer.Back());
        }

        [Fact]
        public void CopyOnPushBack()
        {
            var slidingBuffer = new SlidingBuffer<float>(2, 5);

            // Fill Initial Buffer
            slidingBuffer.PushBack(1f);
            slidingBuffer.PushBack(2f);
            slidingBuffer.PushBack(3f);
            slidingBuffer.PushBack(4f);
            slidingBuffer.PushBack(5f);

            // Copy will occur here.
            slidingBuffer.PushBack(6f);
            Assert.Equal(5f, slidingBuffer.Front());
            Assert.Equal(6f, slidingBuffer.Back());

            // Check buffer slides again after copy.
            slidingBuffer.PushBack(7f);
            Assert.Equal(6f, slidingBuffer.Front());
            Assert.Equal(7f, slidingBuffer.Back());
        }

        [Fact]
        public void ReturnsValidWindow()
        {
            var slidingBuffer = new SlidingBuffer<float>(2, 5);

            // Empty Window
            var window = slidingBuffer.GetWindow();
            Assert.Equal(0, window.Length);

            // Partially Full Window
            slidingBuffer.PushBack(1f);
            window = slidingBuffer.GetWindow();
            Assert.Equal(1, window.Length);
            Assert.Equal(1f, window[0]);

            // Full Window
            slidingBuffer.PushBack(2f);
            window = slidingBuffer.GetWindow();
            Assert.Equal(2, window.Length);
            Assert.Equal(1f, window[0]);
            Assert.Equal(2f, window[1]);

            // Window has slid.
            slidingBuffer.PushBack(3f);
            window = slidingBuffer.GetWindow();
            Assert.Equal(2, window.Length);
            Assert.Equal(2f, window[0]);
            Assert.Equal(3f, window[1]);

            // Window has Overflown.
            slidingBuffer.PushBack(4f);
            slidingBuffer.PushBack(5f);
            slidingBuffer.PushBack(6f); 
            
            window = slidingBuffer.GetWindow();
            Assert.Equal(2, window.Length);
            Assert.Equal(5f, window[0]);
            Assert.Equal(6f, window[1]);
        }
    }
}
