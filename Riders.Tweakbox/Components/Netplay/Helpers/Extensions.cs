using System;
using DotNext.Buffers;
using LiteNetLib;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;

namespace Riders.Tweakbox.Components.Netplay.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Utility for hosts sending arrays of data to other players.
        /// Returns a span of player indices to exclude (all ports local to that player) given <see cref="PlayerData"/> and an existing <see cref="Span{T}"/> to slice
        /// </summary>
        public static Span<byte> GetExcludeIndices(this PlayerData data, Span<byte> excludeIndexBuffer)
        {
            var excludeIndices = excludeIndexBuffer.Slice(0, data.NumPlayers);

            for (int x = 0; x < excludeIndices.Length; x++)
                excludeIndices[x] = (byte) (data.PlayerIndex + x);

            return excludeIndices;
        }

        /// <summary>
        /// Returns a disposable slice of all items except those in the provided indices.
        /// </summary>
        /// <param name="source">The source array from which to make the slice.</param>
        /// <param name="indicesToExclude">Array indices to exclude.</param>
        public static ArrayRental<T> GetItemsWithoutIndices<T>(T[] source, Span<byte> indicesToExclude)
        {
            return GetItemsWithoutIndices(source.AsSpan(), indicesToExclude);
        }

        /// <summary>
        /// Returns a disposable slice of all items except those in the provided indices.
        /// </summary>
        /// <param name="source">The source array from which to make the slice.</param>
        /// <param name="indicesToExclude">Array indices to exclude.</param>
        public static ArrayRental<T> GetItemsWithoutIndices<T>(Span<T> source, Span<byte> indicesToExclude)
        {
            var rental = new ArrayRental<T>(source.Length - indicesToExclude.Length);
            int insertIndex = 0;
            for (int x = 0; x < source.Length; x++)
            {
                if (indicesToExclude.Contains((byte)x))
                    continue;

                rental[insertIndex] = source[x];
                insertIndex += 1;
            }

            return rental;
        }

        /// <summary>
        /// Returns a disposable slice of all items except those in the provided indices.
        /// </summary>
        /// <param name="source">The source array from which to make the slice.</param>
        /// <param name="indicesToExclude">Array indices to exclude.</param>
        /// <param name="discardTimeout">Timeout before the item should not be considered anymore.</param>
        public static ArrayRental<T> GetItemsWithoutIndices<T>(Span<Timestamped<T>> source, Span<byte> indicesToExclude, int discardTimeout)
        {
            var rental = new ArrayRental<T>(source.Length - indicesToExclude.Length);
            int insertIndex = 0;
            for (int x = 0; x < source.Length; x++)
            {
                if (indicesToExclude.Contains((byte)x) || source[x].IsDiscard(discardTimeout))
                    continue;

                rental[insertIndex] = source[x].Value;
                insertIndex += 1;
            }

            return rental;
        }

        /// <summary>
        /// Merges an existing merge-able item if it has not been used or discarded, else replaces it.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cache">The array storing the cached values.</param>
        /// <param name="playerIndex">The player index.</param>
        /// <param name="discardTimeout">Timeout before the item should not be considered anymore.</param>
        public static void ReplaceOrSetCurrentCachedItem<T>(T? item, Timestamped<Used<T>>[] cache, int playerIndex, int discardTimeout) where T : struct, IMergeable<T>
        {
            if (!item.HasValue)
                return;

            ref var currentFlags = ref cache[playerIndex];
            if (currentFlags.IsDiscard(discardTimeout) || currentFlags.Value.IsUsed)
            {
                currentFlags = new Timestamped<Used<T>>(item.Value);
            }
            else
            {
                currentFlags.Refresh();
                currentFlags.Value.Value.Merge(item.Value);
            }
        }

        /// <summary>
        /// Merges an existing merge-able item if it has not been used or discarded, else replaces it.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cache">The array storing the cached values.</param>
        /// <param name="playerIndex">The player index.</param>
        /// <param name="discardTimeout">Timeout before the item should not be considered anymore.</param>
        public static void ReplaceOrSetCurrentCachedItem<T>(T? item, Timestamped<T>[] cache, int playerIndex, int discardTimeout) where T : struct, IMergeable<T>
        {
            if (!item.HasValue)
                return;

            ref var currentFlags = ref cache[playerIndex];
            if (currentFlags.IsDiscard(discardTimeout))
                currentFlags = new Timestamped<T>(item.Value);
            else
            {
                currentFlags.Refresh();
                currentFlags.Value.Merge(item.Value);
            }
        }

        /// <summary>
        /// Fills a span with all indices that should be excluded based on remote player's controlled players.
        /// </summary>
        /// <param name="state">State belonging to the host.</param>
        /// <param name="peer">The peer to exclude.</param>
        /// <param name="excludeIndexBuffer">The buffer to slice.</param>
        /// <returns>Sliced buffer with all indices to exclude removed.</returns>
        public static Span<byte> GetExcludeIndices(HostState state, NetPeer peer, Span<byte> excludeIndexBuffer)
        {
            var playerData = state.ClientMap.GetPlayerData(peer);
            return playerData.GetExcludeIndices(excludeIndexBuffer);
        }
    }
}