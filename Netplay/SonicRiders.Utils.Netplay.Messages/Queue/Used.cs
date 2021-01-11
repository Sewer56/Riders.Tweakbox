namespace Riders.Netplay.Messages.Queue
{
    /// <summary>
    /// A structure type which determines if an action has been performed on the underlying value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Used<T>
    {
        /// <summary>
        /// True if the value has been used.
        /// </summary>
        public bool IsUsed;

        /// <summary>
        /// The value.
        /// </summary>
        public T Value;

        public Used(bool isUsed, T value)
        {
            IsUsed = isUsed;
            Value = value;
        }

        /// <inheritdoc />
        public Used(T value) : this()
        {
            Value = value;
        }
        
        /// <summary>
        /// Gets the value, returning true if it has been used before, else false.
        /// </summary>
        public T UseValue()
        {
            IsUsed = true;
            return Value;
        }

        public static implicit operator Used<T>(T d) => new Used<T>(d);
        public static implicit operator T(Used<T> d) => d.Value;
    }
}
