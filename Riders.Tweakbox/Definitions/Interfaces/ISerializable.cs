using System;

namespace Riders.Tweakbox.Definitions.Interfaces
{
    /// <summary>
    /// Represents a structure that can be converted to binary and back.
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// Converts the structure to a set of bytes.
        /// </summary>
        byte[] ToBytes();

        /// <summary>
        /// Populates a structure given a set of bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>Source span advanced by the number of bytes read.</returns>
        Span<byte> FromBytes(Span<byte> bytes);
    }
}