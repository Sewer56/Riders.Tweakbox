using System;
namespace Riders.Tweakbox.Definitions.Interfaces;

/// <summary>
/// Represents a structure that can be converted to binary and back.
/// Deserialization done in-place, i.e. without creating new instance.
/// </summary>
public interface ISerializable
{
    /// <summary>
    /// Executed when the current configuration is updated.
    /// (When FromBytes is executed)
    /// </summary>
    public Action ConfigUpdated { get; set; }

    /// <summary>
    /// Converts the structure to a set of bytes.
    /// </summary>
    byte[] ToBytes();

    /// <summary>
    /// Populates a structure given a set of bytes.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>Source span advanced by the number of bytes read.</returns>
    void FromBytes(Span<byte> bytes);
}
