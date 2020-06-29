using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Riders.Tweakbox.Misc
{
    public struct Enum<T> where T : unmanaged, Enum
    {
        public T Value;

        public Enum(T value) => Value = value;
        public Enum(long value) => Value = Unsafe.As<long, T>(ref value);

        private long AsLong()
        {
            var sizeOf = Unsafe.SizeOf<T>();
            switch (sizeOf)
            {
                case sizeof(byte):
                    return Unsafe.As<T, byte>(ref Value);

                case sizeof(short):
                    return Unsafe.As<T, short>(ref Value);

                case sizeof(int):
                    return Unsafe.As<T, int>(ref Value);

                case sizeof(long):
                    return (int)Unsafe.As<T, long>(ref Value);

                default:
                    Debug.WriteLine("Warning: We are in the future of 128-bit and beyond. Please consider updating me!");
                    return (int)Unsafe.As<T, long>(ref Value);
            }
        }

        public static implicit operator Enum<T>(bool value) => new Enum<T>(value ? 1 : 0);
        public static implicit operator Enum<T>(int value) => new Enum<T>(value);
        public static implicit operator Enum<T>(long value) => new Enum<T>(value);
        public static implicit operator Enum<T>(T value) { return new Enum<T>(value); }
        public static implicit operator T(Enum<T> value) => value.Value;
        public static implicit operator long(Enum<T> value) => (int)value.AsLong();
        public static implicit operator int(Enum<T> value) => (int) value.AsLong();
        public static implicit operator bool(Enum<T> value) { return value.AsLong() == (long) 1; }
    }
}