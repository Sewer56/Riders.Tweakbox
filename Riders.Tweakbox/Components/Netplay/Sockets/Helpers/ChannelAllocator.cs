using Sewer56.Imgui.Utilities;
namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers;

/// <summary>
/// Utility class for allocating unique channel IDs to use with the networking library.
/// </summary>
public class ChannelAllocator
{
    /// <summary>
    /// Number of channels allocated.
    /// </summary>
    public int NumChannels { get; private set; }

    /// <summary>
    /// Index of first channel.
    /// </summary>
    public int FirstIndex { get; private set; }

    private bool[] _allocatedSlots;

    /// <summary>
    /// Creates a channel allocator used for allocating channels for different packets.
    /// </summary>
    /// <param name="numChannels">The number of channels to allocate.</param>
    /// <param name="firstIndex">The first index to start allocating from. Default for this is 1 because default channel is 0.</param>
    public ChannelAllocator(int numChannels, int firstIndex = 1)
    {
        NumChannels = numChannels;
        FirstIndex = firstIndex;
        _allocatedSlots = new bool[numChannels];
    }

    /// <summary>
    /// Requests an individual unique slot index.
    /// </summary>
    /// <returns>Slot number, or -1 if not found.</returns>
    public int GetChannel()
    {
        var channelId = _allocatedSlots.IndexOf(FirstIndex, x => x == false);
        if (channelId != -1)
            _allocatedSlots[channelId] = true;

        return channelId;
    }

    /// <summary>
    /// Releases an individual slot index.
    /// </summary>
    /// <param name="index">The slot to release from the slot mapper.</param>
    public void ReleaseChannel(int index)
    {
        _allocatedSlots[index] = false;
    }
}
