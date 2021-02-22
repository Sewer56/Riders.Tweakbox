using System;
using System.Collections.Generic;

namespace Riders.Netplay.Messages.Helpers
{
    /// <summary>
    /// A simple wrapper for value types around an array with a dictionary-like interface.
    /// No bounds checks. Assumes all keys hash to a value within bounds (hint: int key).
    /// </summary>
    public struct StructArrayMap<TKey, TValue> where TValue : struct
    {
        private TValue?[] _values;

        public int Count { get; private set; }

        public StructArrayMap(int numberOfElements)
        {
            _values = new TValue?[numberOfElements];
            Count = 0;
        }

        public void Clear()
        {
            Array.Fill(_values, null);
            Count = 0;
        }

        public bool ContainsKey(TKey key) => _values[key.GetHashCode()].HasValue;
        public bool Remove(TKey key, out TValue value)
        {
            bool hasValue = ContainsKey(key);
            value = _values[key.GetHashCode()].GetValueOrDefault();
            _values[key.GetHashCode()] = null;

            Count -= Convert.ToInt32(hasValue);
            return hasValue;
        }

        public bool Remove(TKey key)
        {
            bool hasValue = ContainsKey(key);
            _values[key.GetHashCode()] = null;

            Count -= Convert.ToInt32(hasValue);
            return hasValue;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool hasValue = ContainsKey(key);
            value = _values[key.GetHashCode()].GetValueOrDefault();
            return hasValue;
        }

        public TValue? this[TKey key]
        {
            get => _values[key.GetHashCode()];
            set
            {
                bool hasValue = ContainsKey(key);
                if (!hasValue)
                    Count += 1;

                _values[key.GetHashCode()] = value;
            }
        }

        public IEnumerator<TValue> Values()
        {
            for (int x = 0; x < _values.Length; x++)
            {
                if (_values[x].HasValue)
                    yield return _values[x].Value;
            }
        }
    }
}