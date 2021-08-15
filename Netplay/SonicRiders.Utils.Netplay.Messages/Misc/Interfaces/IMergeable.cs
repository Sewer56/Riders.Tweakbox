namespace Riders.Netplay.Messages.Misc.Interfaces;

/// <summary>
/// An interface for elements where new values can be merged with existing values.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMergeable<T>
{
    /// <summary>
    /// Merges a new item with the current struct
    /// </summary>
    /// <param name="toMerge">Item to merge with the existing values.</param>
    void Merge(in T toMerge);
}
