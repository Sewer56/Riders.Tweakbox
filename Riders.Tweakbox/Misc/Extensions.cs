using System;
using System.Collections.Generic;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Tweakbox.Components.Netplay;

namespace Riders.Tweakbox.Misc
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the index of an item that satisfies the given condition.
        /// </summary>
        /// <param name="source">Source of the items.</param>
        /// <param name="predicate">Returns true if desired item.</param>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                    return index;
                
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Turns a player config into user data to send over the web.
        /// </summary>
        public static HostPlayerData ToHostPlayerData(this NetplayImguiConfig config)
        {
            return new HostPlayerData()
            {
                Name = config.PlayerName.Text,
                PlayerIndex = 0
            };
        }

        public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*) 0x0;
    }
}
