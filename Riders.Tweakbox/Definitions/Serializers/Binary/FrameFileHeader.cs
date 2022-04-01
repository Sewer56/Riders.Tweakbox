using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary;

/// <summary>
/// Frame type that represents a file header.
/// </summary>
public struct FrameFileHeader
{
    /// <summary>
    /// Node signifying start of file.
    /// </summary>
    public const int NODE_FILESTART = 0x55333C49; // Arrrrrrrrrrg 
    public const int FrameSizeBits = 64; // For writing.

    /// <summary>
    /// Id of the file header node.
    /// </summary>
    public int Id;

    /// <summary>
    /// Size of the file in bits.
    /// </summary>
    public int SizeInBits;

    public FrameFileHeader()
    {
        Id = NODE_FILESTART;
        SizeInBits = 0;
    }

    public FrameFileHeader(int sizeInBits)
    {
        Id = NODE_FILESTART;
        SizeInBits = sizeInBits;
    }

    /// <summary>
    /// Reads the frame header from a stream.
    /// If unsuccessful, stream is not advanced.
    /// </summary>
    /// <typeparam name="TStreamType">Type associated with bitstream.</typeparam>
    /// <param name="bitStream">The bitstream to try read frame header from.</param>
    /// <param name="header">The frame read from the stream.</param>
    /// <returns>True if a frame was read, else false.</returns>
    public static bool TryReadFrameHeader<TStreamType>(ref BitStream<TStreamType> bitStream, out FrameFileHeader header) where TStreamType : IByteStream
    {
        var originalIndex = bitStream.BitIndex;
        header = default;
        if (bitStream.Read<int>() != NODE_FILESTART)
        {
            bitStream.BitIndex = originalIndex;
            return false;
        }

        bitStream.BitIndex = originalIndex;
        header.Id = bitStream.Read<int>();
        header.SizeInBits = bitStream.Read<int>();
        return true;
    }

    /// <summary>
    /// Writes the header to a given stream.
    /// </summary>
    /// <typeparam name="TStreamType"></typeparam>
    /// <param name="bitStream">The stream to write the frame header to.</param>
    /// <param name="header">The header to write.</param>
    public static void Write<TStreamType>(ref BitStream<TStreamType> bitStream, FrameFileHeader header) where TStreamType : IByteStream
    {
        bitStream.Write(header.Id);
        bitStream.Write(header.SizeInBits);
    }
}