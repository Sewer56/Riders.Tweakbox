using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using DearImguiSharp;
using Reloaded.Memory.Interop;

namespace Sewer56.Imgui.Utilities
{
    /// <summary>
    /// Encapsulates the creation of native memory to store text input data.
    /// </summary>
    public unsafe class TextInputData
    {
        public sbyte* Pointer => _textInput.Pointer;
        public string Text => GetText();
        public ulong SizeOfData { get; private set; }

        private Pinnable<sbyte> _textInput;

        public TextInputData(int numberOfCharacters)
        {
            SizeOfData = (ulong) (numberOfCharacters * sizeof(int));
            _textInput = new Pinnable<sbyte>(new sbyte[SizeOfData]);
        }

        public string GetText()
        {
            var text = Encoding.UTF8.GetString((byte*)Pointer, (int)SizeOfData);
            int index = text.IndexOf('\0');
            if (index >= 0)
                text = text.Remove(index);

            return text;
        }

        /// <summary>
        /// Custom filter for functions such as <see cref="ImGui.InputText"/>
        /// </summary>
        public static int FilterValidPathCharacters(IntPtr ptr)
        {
            const int numberOfChars = 2;
            var data  = new ImGuiInputTextCallbackData((void*) ptr);

            // Source
            var characterShort = data.EventChar;
            var byteSpan = new ReadOnlySpan<byte>(&characterShort, sizeof(short));

            // Destination (4 bytes on stack, API needs null terminator so 2 is not enough!)
            var chars = stackalloc char[numberOfChars];
            var charsSpan = new Span<char>(chars, numberOfChars);
            
            Encoding.UTF8.GetChars(byteSpan, charsSpan);
            return PathSanitizer.IsCharacterValid(charsSpan[0]) ? 0 : 1;
        }

        public static class PathSanitizer
        {
            public static readonly char[] InvalidCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

            /// <summary>
            /// True if character is a valid file path character, else false.
            /// </summary>
            public static bool IsCharacterValid(char character) => !InvalidCharacters.Contains(character);
        }
    }
}
