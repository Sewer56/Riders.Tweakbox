using System;
using System.Collections.Generic;
using System.Text;

namespace Sewer56.Imgui.Utilities
{
    public static class EncodingExtensions
    {
        /// <summary>
        /// Converts an individual character to a char given the key code of the character.
        /// </summary>
        /// <param name="utf8">The encoding.</param>
        /// <param name="character">The character key code.</param>
        public static unsafe char ToCharacter(this Encoding utf8, ushort character)
        {
            const int numberOfChars = 2;

            // Source
            var byteSpan = new ReadOnlySpan<byte>(&character, sizeof(short));

            // Destination (4 bytes on stack, API needs null terminator so 2 is not enough!)
            var chars     = stackalloc char[numberOfChars];
            var charsSpan = new Span<char>(chars, numberOfChars);
            utf8.GetChars(byteSpan, charsSpan);
            return charsSpan[0];
        }
    }
}
