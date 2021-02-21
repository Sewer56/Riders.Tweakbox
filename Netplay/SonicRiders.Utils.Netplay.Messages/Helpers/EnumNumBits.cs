using System;
using System.Linq;
using DotNext;
using EnumsNET;
using Riders.Netplay.Messages.Misc;

namespace Riders.Netplay.Messages.Helpers
{
    /// <summary>
    /// Gets the number of bits for a given enumerable type.
    /// </summary>
    public static class EnumNumBits<T> where T : struct, Enum
    {
        /// <summary>
        /// Number of bits reserved for the message type.
        /// </summary>
        public static readonly int Number = Utilities.GetMinimumNumberOfBits(Enums.GetValues<T>().Last().ToInt32());
    }
}