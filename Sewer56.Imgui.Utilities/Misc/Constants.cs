using System.Runtime.CompilerServices;
using DearImguiSharp;

namespace Sewer56.Imgui.Misc
{
    public static unsafe class Constants
    {
        public static bool True = true;
        public static float Spacing = 10.0f;

        public static ImVec2 ButtonSizeThin = new ImVec2() { X = 80, Y = 0 };
        public static ImVec2 ButtonSize = new ImVec2() { X = 120, Y = 0 };
        public static ImVec2 DefaultVector2 = new ImVec2() { X = 0, Y = 0 };

        /// <summary>
        /// Returns a null reference to type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ref T NullReference<T>() => ref Unsafe.AsRef<T>((void*)0x0);
    }
}
