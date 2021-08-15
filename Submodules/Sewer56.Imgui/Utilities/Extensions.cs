using System;
using System.Collections.Generic;
namespace Sewer56.Imgui.Utilities;

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
    /// Returns the index of an item that satisfies the given condition.
    /// </summary>
    /// <param name="source">Source of the items.</param>
    /// <param name="firstIndex">The first index to check.</param>
    /// <param name="predicate">Returns true if desired item.</param>
    public static int IndexOf<TSource>(this IReadOnlyList<TSource> source, int firstIndex, Func<TSource, bool> predicate)
    {
        for (var x = firstIndex; x < source.Count; x++)
        {
            var item = source[x];
            if (predicate.Invoke(item))
                return x;
        }

        return -1;
    }
}
