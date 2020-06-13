using System;
using System.Collections.Generic;
using System.Text;

namespace Riders.Netplay.Messages.Misc
{
    /// <summary>
    /// Represents a singular volatile value.
    /// When you get the value, the underlying value resets itself to default.
    /// </summary>
    public struct Volatile<T> where T : new()
    {
        public bool HasValue { get; private set; }
        private T _value;

        public Volatile(T value)
        {
            _value = value;
            HasValue = true;
        }

        /// <summary>
        /// Get the internal value without clearing it.
        /// </summary>
        public T GetNonvolatile() => _value;

        /// <summary>
        /// Gets a field and erases its current content.
        /// </summary>
        public T Get() 
        {
            var copy = _value;
            _value = new T();
            HasValue = false;
            return copy;
        }

        /// <summary>
        /// Gets a field and replaces the value with a custom default.
        /// </summary>
        public T Get(T defaultValue)
        {
            var copy = _value;
            _value = defaultValue;
            HasValue = false;
            return copy;
        }

        /// <summary>
        /// Sets the value in question.
        /// </summary>
        public void Set(T value)
        {
            _value = value;
            HasValue = true;
        }

        public static explicit operator T(Volatile<T> d) => d.Get();
        public static implicit operator Volatile<T>(T d) => new Volatile<T>(d);
    }
}
