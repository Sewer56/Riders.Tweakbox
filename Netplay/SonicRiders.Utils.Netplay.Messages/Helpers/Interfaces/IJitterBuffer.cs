using System;
using Riders.Netplay.Messages.Misc.Interfaces;

namespace Riders.Netplay.Messages.Helpers.Interfaces
{
    public interface IJitterBuffer<T> where T : struct, ISequenced, IDisposable
    {
        JitterBufferType GetBufferType();
        void Clear();

        /// <summary>
        /// Tries to add a packet to the adaptive jitter buffer queue.
        /// </summary>
        bool TryEnqueue(in T packet);

        /// <summary>
        /// Tries to dequeue a packet from the adaptive buffer.
        /// </summary>
        bool TryDequeue(int playerIndex, out T packet);

        /// <summary>
        /// Sets the size for the new buffer.
        /// </summary>
        /// <param name="newSize">New buffer size.</param>
        void SetBufferSize(int newSize);
    }

    public enum JitterBufferType
    {
        Simple,
        Adaptive,
        Hybrid
    }
}