using System.Collections.Generic;
using EnumsNET;
using LiteNetLib;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Utility class for allocating unique channel IDs to use with individual <see cref="DeliveryMethod"/>(s) of LiteNetLib.
    /// </summary>
    public class LnlChannelAllocator
    {
        /// <summary>
        /// Number of channels allocated.
        /// </summary>
        public int NumChannels { get; private set; }

        /// <summary>
        /// Index of first channel.
        /// </summary>
        public int FirstIndex { get; private set; }

        /// <summary>
        /// Maps all delivery methods to allocators.
        /// </summary>
        private Dictionary<DeliveryMethod, ChannelAllocator> _allocators = new Dictionary<DeliveryMethod, ChannelAllocator>();

        /// <summary>
        /// Creates a channel allocator used for allocating channels for different packets.
        /// </summary>
        /// <param name="numChannels">The number of channels to allocate.</param>
        /// <param name="firstIndex">The first index to start allocating from. Default for this is 1 because default channel is 0.</param>
        public LnlChannelAllocator(int numChannels, int firstIndex = 1)
        {
            NumChannels = numChannels;
            FirstIndex  = firstIndex;

            foreach (var method in Enums.GetValues<DeliveryMethod>())
                _allocators[method] = new ChannelAllocator(numChannels, firstIndex);
        }

        /// <summary>
        /// Requests an individual unique slot index.
        /// </summary>
        /// <returns>Slot number, or -1 if not found.</returns>
        public int GetChannel(DeliveryMethod method) => _allocators[method].GetChannel();

        /// <summary>
        /// Releases an individual slot index.
        /// </summary>
        /// <param name="method">The delivery method.</param>
        /// <param name="index">The slot to release from the slot mapper.</param>
        public void ReleaseChannel(DeliveryMethod method, int index) => _allocators[method].ReleaseChannel(index);
    }
}
