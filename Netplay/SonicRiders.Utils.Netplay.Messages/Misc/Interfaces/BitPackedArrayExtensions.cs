using System;
namespace Riders.Netplay.Messages.Misc.Interfaces;

public static class BitPackedArrayExtensions
{
    /// <summary>
    /// Creates an instance of the parent using the specified elements.
    /// Note: Array must at least contain 1 element.
    /// </summary>
    public static void Set<TParent, T, TOwner>(ref this TParent self, T[] elements, int numElements = -1)
        where TParent : struct, IBitPackedArray<T, TOwner>
        where T : unmanaged, IBitPackable<T>
        where TOwner : struct, IBitPackedArray<T, TOwner>
    {
#if DEBUG
        // Restricted to debug because exceptions prevent inlining.
        if (elements.Length == 0)
            throw new Exception("Array has zero elements.");
#endif

        self.Dispose();
        self.IsPooled = false;
        self.Elements = elements;
        self.NumElements = numElements != -1 ? numElements : elements.Length;
    }

    /// <summary>
    /// Creates an instance of the parent using the specified elements.
    /// Note: Array must at least contain 1 element.
    /// </summary>
    public static TParent Create<TParent, T, TOwner>(ref this TParent self, T[] elements)
        where TParent : struct, IBitPackedArray<T, TOwner>
        where T : unmanaged, IBitPackable<T>
        where TOwner : struct, IBitPackedArray<T, TOwner>
    {
#if DEBUG
        // Restricted to debug because exceptions prevent inlining.
        if (elements.Length == 0)
            throw new Exception("Array has zero elements.");
#endif

        return new TParent
        {
            IsPooled = false,
            Elements = elements,
            NumElements = elements.Length
        };
    }

    /// <summary>
    /// Creates a pooled instance of the parent using the specified elements.
    /// Note: Array must at least contain 1 element.
    /// </summary>
    public static TParent CreatePooled<TParent, T, TOwner>(ref this TParent self, int numElements)
        where TParent : struct, IBitPackedArray<T, TOwner>
        where T : unmanaged, IBitPackable<T>
        where TOwner : struct, IBitPackedArray<T, TOwner>
    {
#if DEBUG
        // Restricted to debug because exceptions prevent inlining.
        if (numElements <= 0)
            throw new Exception("Array has zero elements.");
#endif

        return new TParent
        {
            IsPooled = true,
            Elements = IBitPackedArray<T, TOwner>.SharedPool.Rent(numElements),
            NumElements = numElements
        };
    }

    /// <summary>
    /// Clones the current bit packed array.
    /// </summary>
    public static TParent Clone<TParent, T, TOwner>(ref this TParent self)
        where TParent : struct, IBitPackedArray<T, TOwner>
        where T : unmanaged, IBitPackable<T>
        where TOwner : struct, IBitPackedArray<T, TOwner>
    {
        var result = self.IsPooled ? CreatePooled<TParent, T, TOwner>(ref self, self.NumElements) : Create<TParent, T, TOwner>(ref self, new T[self.NumElements]);
        for (int x = 0; x < result.NumElements; x++)
            result.Elements[x] = self.Elements[x];

        return result;
    }
}
