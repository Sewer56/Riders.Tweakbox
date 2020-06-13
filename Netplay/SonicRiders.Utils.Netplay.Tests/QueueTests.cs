using Riders.Netplay.Messages.Queue;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class QueueTests
    {

        [Fact]
        public void Create()
        {
            var queue = new MessageQueue();
            var reliableQueue = queue.Get<ReliablePacket>();
            reliableQueue.Enqueue(new ReliablePacket() {});

            var reliableQueueRef = queue.Get<ReliablePacket>();
            Assert.Equal(reliableQueue.Count, reliableQueueRef.Count);
        }

        [Fact]
        public void Clear()
        {
            var queue = new MessageQueue();
            var reliableQueue = queue.Get<ReliablePacket>();
            reliableQueue.Enqueue(new ReliablePacket() { });

            queue.Clear<ReliablePacket>();
            reliableQueue = queue.Get<ReliablePacket>();
            Assert.Empty(reliableQueue);
        }

        [Fact]
        public void Equal()
        {
            var queue = new MessageQueue();
            var reliableQueue = queue.Get<ReliablePacket>();
            reliableQueue.Enqueue(new ReliablePacket() { });

            var reliableQueueRef = queue.Get<ReliablePacket>();
            Assert.Equal(reliableQueue, reliableQueueRef);
        }
    }
}
