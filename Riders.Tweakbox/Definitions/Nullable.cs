namespace Riders.Tweakbox.Definitions;

/// <summary>
/// Version of <see cref="System.Nullable{T}"/> with support for ref returns.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Nullable<T>
{
    public bool HasValue;
    public T Value;

    public Nullable(T value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Sets the value if the item does not have a value.
    /// </summary>
    public void SetIfNull(in T value)
    {
        if (HasValue)
            return;

        HasValue = true;
        Value = value;
    }

    public static implicit operator T(Nullable<T> value) => value.Value;
    public static implicit operator Nullable<T>(T value) => new Nullable<T>(value);
}
