using System;
using System.Runtime.InteropServices;

namespace Riders.Tweakbox.Misc
{
    public static class Native
    { 
        [DllImport("kernel32.dll")]
        public static extern bool VirtualUnlock(IntPtr lpAddress, UIntPtr dwSize);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtSetTimerResolution(int desiredResolution, bool setResolution, out int currentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryTimerResolution(out int maximumResolution, out int minimumResolution, out int currentResolution);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /// <summary>
        /// Represents a singular unicode string.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            private IntPtr buffer;

            public unsafe UNICODE_STRING(char* pointer, int length)
            {
                Length = (ushort)(length * 2);
                MaximumLength = (ushort)(Length + 2);
                buffer = (IntPtr) pointer;
            }

            /// <summary>
            /// Returns a string with the contents
            /// </summary>
            /// <returns></returns>
            public override unsafe string ToString()
            {
                if (buffer != IntPtr.Zero)
                    return new string((char*)buffer, 0, Length / sizeof(char));

                return String.Empty;
            }
        }

        public const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        [Flags]
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,
        }
    }
}
