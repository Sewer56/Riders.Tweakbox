using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sewer56.Imgui.Utilities
{
    public static class PathSanitizer
    {
        public static readonly char[] InvalidCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

        /// <summary>
        /// True if character is a valid file path character, else false.
        /// </summary>
        public static bool IsCharacterValid(char character) => !InvalidCharacters.Contains(character);
    }
}
