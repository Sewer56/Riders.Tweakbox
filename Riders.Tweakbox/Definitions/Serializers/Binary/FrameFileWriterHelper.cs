using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary;

/// <summary>
/// Helper for easier writing of frame files.
/// Usage:
///     `var writer = new FrameFileWriterHelper(ref bitStream);`
///     // Write a bunch of frames using FrameGeneric.Write
///     frameFileWriter.Write(ref bitStream);
/// </summary>
public unsafe struct FrameFileWriterHelper<TStreamType> where TStreamType : IByteStream
{
    private int _headerIndex;
    private int _afterHeaderIndex;

    public FrameFileWriterHelper(ref BitStream<TStreamType> bitStream)
    {
        _headerIndex = bitStream.BitIndex;
        _afterHeaderIndex = _headerIndex + FrameFileHeader.FrameSizeBits;
        bitStream.BitIndex = _afterHeaderIndex;
    }

    public void Write(ref BitStream<TStreamType> bitStream)
    {
        var currentIndex    = bitStream.BitIndex;
        var size            = currentIndex - _afterHeaderIndex;

        // Write header to stream, and seek back.
        bitStream.BitIndex = _headerIndex;
        FrameFileHeader.Write(ref bitStream, new FrameFileHeader(size));
        bitStream.BitIndex = currentIndex;
    }
}