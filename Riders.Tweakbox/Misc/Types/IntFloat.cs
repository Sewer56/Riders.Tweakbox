using System.Runtime.CompilerServices;

namespace Riders.Tweakbox.Misc.Types
{
    /// <summary>
    /// Floating point value returned as integer so it doesn't get returned in st(0) by C# code.
    /// </summary>
    public struct IntFloat
    {
        public int Value;

        public IntFloat(float value)
        {
            Value = Unsafe.As<float, int>(ref value);
        }

        public static implicit operator IntFloat(float value) => new IntFloat(value);
    }
}