using System;
using System.Collections.Generic;

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
    }
}
