using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Unreliable;
using Xunit;
namespace Riders.Netplay.Messages.Tests;

public class JitterBufferTests
{
    // Note: Tests below are based on these constants.
    public const int TimeUntilDequeue = 3;
    public JitterBuffer<Messages.UnreliablePacket> Buffer = new JitterBuffer<Messages.UnreliablePacket>(TimeUntilDequeue);

    [Fact]
    public void DuplicatesOverwrite()
    {
        // Same Sequence Number
        using var unreliable = new Messages.UnreliablePacket(8, 1);
        Assert.True(Buffer.TryEnqueue(unreliable));
        Assert.True(Buffer.TryEnqueue(unreliable));
        Assert.Equal(1, Buffer.PacketCount);
    }

    [Fact]
    public void DequeueAccuratelyDelayed()
    {
        using var unreliableFirst = new Messages.UnreliablePacket(8, 1);
        using var unreliableSecond = new Messages.UnreliablePacket(8, 2);
        using var unreliableThird = new Messages.UnreliablePacket(8, 3);

        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.True(Buffer.TryEnqueue(unreliableSecond));
        Assert.False(Buffer.TryDequeue(out var value));

        Assert.True(Buffer.TryEnqueue(unreliableThird));
        Assert.True(Buffer.TryDequeue(out value));
        Assert.Equal(2, Buffer.PacketCount);
    }

    [Fact]
    public void StartsQueueingAtRightTime()
    {
        using var unreliableFirst = new Messages.UnreliablePacket(8, 1);
        using var unreliableSecond = new Messages.UnreliablePacket(8, 2);
        using var unreliableThird = new Messages.UnreliablePacket(8, 3);

        using var unreliableFourth = new Messages.UnreliablePacket(8, 4);
        using var unreliableFifth = new Messages.UnreliablePacket(8, 5);
        using var unreliableSixth = new Messages.UnreliablePacket(8, 6);

        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.False(Buffer.TryDequeue(out var value));

        Assert.True(Buffer.TryEnqueue(unreliableSecond));
        Assert.False(Buffer.TryDequeue(out value));

        Assert.False(Buffer.IsDeQueueing);
        Assert.True(Buffer.TryEnqueue(unreliableThird));
        Assert.True(Buffer.TryDequeue(out value));
        Assert.True(Buffer.IsDeQueueing);
        Assert.Equal(2, Buffer.PacketCount);
    }

    [Fact]
    public void SupportsOverflow()
    {
        var overflowValue = new Messages.UnreliablePacket().MaxValue + 1;

        using var unreliableFirst = new Messages.UnreliablePacket(8, overflowValue - 1);
        using var unreliableSecond = new Messages.UnreliablePacket(8, overflowValue);
        using var unreliableThird = new Messages.UnreliablePacket(8, overflowValue + 1);
        using var unreliableFourth = new Messages.UnreliablePacket(8, overflowValue + 2);
        using var unreliableFifth = new Messages.UnreliablePacket(8, overflowValue + 3);

        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.True(Buffer.TryEnqueue(unreliableSecond));
        Assert.True(Buffer.TryEnqueue(unreliableThird));
        Assert.True(Buffer.TryEnqueue(unreliableFourth));
        Assert.True(Buffer.TryEnqueue(unreliableFifth));

        Assert.True(Buffer.TryDequeue(out var firstCopy));
        Assert.True(Buffer.TryDequeue(out var secondCopy));
        Assert.True(Buffer.TryDequeue(out var thirdCopy));

        Assert.Equal(overflowValue - 1, unreliableFirst.SequenceNo);
        Assert.Equal(overflowValue - 1, firstCopy.SequenceNo);

        Assert.Equal(0, unreliableSecond.SequenceNo);
        Assert.Equal(0, secondCopy.SequenceNo);

        Assert.Equal(1, unreliableThird.SequenceNo);
        Assert.Equal(1, thirdCopy.SequenceNo);
    }

    [Fact]
    public void SupportsOutOfOrderEnqueue()
    {
        using var unreliableFirst = new Messages.UnreliablePacket(8);
        using var unreliableSecond = new Messages.UnreliablePacket(8, 1);
        using var unreliableThird = new Messages.UnreliablePacket(8, 2);

        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.True(Buffer.TryEnqueue(unreliableThird));
        Assert.True(Buffer.TryEnqueue(unreliableSecond));

        Assert.True(Buffer.TryDequeue(out var firstCopy));
        Assert.True(Buffer.TryDequeue(out var secondCopy));
        Assert.True(Buffer.TryDequeue(out var thirdCopy));

        Assert.Equal(unreliableFirst.SequenceNo, firstCopy.SequenceNo);
        Assert.Equal(unreliableSecond.SequenceNo, secondCopy.SequenceNo);
        Assert.Equal(unreliableThird.SequenceNo, thirdCopy.SequenceNo);
    }

    [Fact]
    public void SupportsMissingSequenceValueRealtime()
    {
        using var unreliableFirst = new Messages.UnreliablePacket(8);
        using var unreliableSecond = new Messages.UnreliablePacket(8, 1);
        using var unreliableThird = new Messages.UnreliablePacket(8, 2);
        using var unreliableFourth = new Messages.UnreliablePacket(8, 3);
        using var unreliableFifth = new Messages.UnreliablePacket(8, 4);
        using var unreliableSixth = new Messages.UnreliablePacket(8, 5);

        // Oops! Third gets lost in transit
        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.True(Buffer.TryEnqueue(unreliableSecond));
        // Missing 3rd
        Assert.True(Buffer.TryEnqueue(unreliableFourth));
        Assert.True(Buffer.TryEnqueue(unreliableFifth));
        Assert.True(Buffer.TryEnqueue(unreliableSixth));

        // But now there's a gap! Will our buffer return? Let's see!
        Assert.True(Buffer.TryDequeue(out var firstCopy));
        Assert.True(Buffer.TryDequeue(out var secondCopy));
        Assert.False(Buffer.TryDequeue(out var thirdCopy));

        Assert.True(Buffer.TryDequeue(out var fourthCopy));
        Assert.True(Buffer.TryDequeue(out var fifthCopy));
        Assert.True(Buffer.TryDequeue(out var sixthCopy));
    }

    [Fact]
    public void DiscardsOutdatedValues()
    {
        var bufferHalfSize = Buffer.Size / 2;

        // Populate Buffer
        for (int x = 0; x < bufferHalfSize; x++)
        {
            var packet = new Messages.UnreliablePacket() { Header = new UnreliablePacketHeader(1, x) };
            Assert.True(Buffer.TryEnqueue(packet));
        }

        // Now buffer should discard the other half.
        for (int x = bufferHalfSize; x < Buffer.Size; x++)
        {
            var packet = new Messages.UnreliablePacket() { Header = new UnreliablePacketHeader(1, x) };
            Assert.False(Buffer.TryEnqueue(packet));
        }
    }

    [Fact]
    public void SenderCannotOverwhelmReceiverInLowLatencyMode()
    {
        // Populate Buffer
        Buffer.LowLatencyMode = true;
        for (int x = 0; x < Buffer.Size; x++)
        {
            var packet = new Messages.UnreliablePacket() { Header = new UnreliablePacketHeader(1, x) };
            Assert.True(Buffer.TryEnqueue(packet));
        }

        Assert.Equal(TimeUntilDequeue, Buffer.PacketCount);
    }

    [Fact]
    public void SupportsMissingSequenceValue()
    {
        using var unreliableFirst = new Messages.UnreliablePacket(8);
        using var unreliableSecond = new Messages.UnreliablePacket(8, 1);
        using var unreliableThird = new Messages.UnreliablePacket(8, 2);
        using var unreliableFourth = new Messages.UnreliablePacket(8, 3);

        using var dummyOne = new Messages.UnreliablePacket(8, 4);
        using var dummyTwo = new Messages.UnreliablePacket(8, 5);
        using var dummyThree = new Messages.UnreliablePacket(8, 6);

        // Oops! Second gets lost in transit
        Assert.True(Buffer.TryEnqueue(unreliableFirst));
        Assert.True(Buffer.TryEnqueue(unreliableThird));
        Assert.True(Buffer.TryEnqueue(unreliableFourth));

        Assert.True(Buffer.TryEnqueue(dummyOne));
        Assert.True(Buffer.TryEnqueue(dummyTwo));
        Assert.True(Buffer.TryEnqueue(dummyThree));

        Assert.True(Buffer.TryDequeue(out var firstCopy));
        Assert.False(Buffer.TryDequeue(out var notSecond));
        Assert.True(Buffer.TryDequeue(out var thirdCopy));

        Assert.Equal(unreliableFirst.SequenceNo, firstCopy.SequenceNo);
        Assert.NotEqual(notSecond.SequenceNo, unreliableSecond.SequenceNo);
        Assert.Equal(unreliableThird.SequenceNo, thirdCopy.SequenceNo);
    }
}
