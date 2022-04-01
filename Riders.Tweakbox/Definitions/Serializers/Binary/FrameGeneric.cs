using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary;

public struct FrameGeneric
{
    public const int CurrentVersion = 0;
    public const int FrameSizeBits  = IdBits + VersionBits + SizeBits; // For writing
    public const int IdBits         = 32;
    public const int VersionBits    = 2;  // Max: 4
    public const int SizeBits       = 30; // Max: ~134MB

    /// <summary>
    /// The type of the node used, e.g. 
    /// </summary>
    public int Id;

    /// <summary>
    /// Version of the frame format.
    /// Will probably never be used, but just in case.
    /// </summary>
    public byte Version;

    /// <summary>
    /// Size of payload (in bits).
    /// </summary>
    public int SizeInBits;
    
    /// <summary>
    /// Reads the frame header from a stream.
    /// </summary>
    /// <typeparam name="TStreamType">Type associated with bitstream.</typeparam>
    /// <param name="bitStream">The bitstream to try read frame header from.</param>
    /// <param name="frame">The frame read from the stream.</param>
    /// <returns>Number of bits read.</returns>
    public static int Read<TStreamType>(ref BitStream<TStreamType> bitStream, out FrameGeneric frame) where TStreamType : IByteStream
    {
        frame = default;
        var originalOffset = bitStream.BitIndex;
        frame.Id           = bitStream.Read<int>();
        frame.Version      = bitStream.Read<byte>(VersionBits);
        frame.SizeInBits   = bitStream.Read<int>(SizeBits);
        return bitStream.BitIndex - originalOffset;
    }

    /// <summary>
    /// Writes the frame header to a stream.
    /// </summary>
    /// <typeparam name="TStreamType">Type associated with bitstream.</typeparam>
    /// <param name="bitStream">The bitstream to try read frame header from.</param>
    /// <param name="frame">The frame header to write.</param>
    public static void Write<TStreamType>(ref BitStream<TStreamType> bitStream, ref FrameGeneric frame) where TStreamType : IByteStream
    {
        bitStream.Write(frame.Id);
        bitStream.Write(frame.Version, VersionBits);
        bitStream.Write(frame.SizeInBits, SizeBits);
    }
    
    /// <summary>
    /// Writes a frame to the given BitStream.
    /// </summary>
    /// <typeparam name="TData">The type of data to write.</typeparam>
    /// <typeparam name="TStreamType">Type associated with the bitstream./</typeparam>
    /// <param name="bitStream">The stream to write the data to.</param>
    /// <param name="data">The data to write to the stream.</param>
    /// <param name="serializeFn">Function used to serialize the data.</param>
    /// <param name="id">Optional. Can be used to override written ID.</param>
    public static void Write<TData, TStreamType>(ref BitStream<TStreamType> bitStream, ref TData data, WriteFrameData<TData, TStreamType> serializeFn, int? id = default) where TStreamType : IByteStream
    {
        Write_Begin(ref bitStream, out var dataStartOffset, out var frameOffset);
        Write_End(ref bitStream, serializeFn(ref bitStream, ref data), id, dataStartOffset, frameOffset);
    }

    /// <summary>
    /// Writes a frame to the given BitStream.
    /// </summary>
    /// <typeparam name="TData">The type of data to write.</typeparam>
    /// <typeparam name="TStreamType">Type associated with the bitstream./</typeparam>
    /// <param name="bitStream">The stream to write the data to.</param>
    /// <param name="data">The data to write to the stream.</param>
    /// <param name="serializeFn">Function used to serialize the data.</param>
    /// <param name="id">Optional. Can be used to override written ID.</param>
    public static void Write<TData, TStreamType>(ref BitStream<TStreamType> bitStream, Span<TData> data, WriteFrameDataSpan<TData, TStreamType> serializeFn, int? id = default) where TStreamType : IByteStream
    {
        Write_Begin(ref bitStream, out var dataStartOffset, out var frameOffset);
        Write_End(ref bitStream, serializeFn(ref bitStream, ref data), id, dataStartOffset, frameOffset);
    }

    private static void Write_Begin<TStreamType>(ref BitStream<TStreamType> bitStream, out int dataStartOffset, out int frameOffset) where TStreamType : IByteStream
    {
        // Skip frame data.
        frameOffset = bitStream.BitIndex;
        bitStream.BitIndex += FrameSizeBits;

        // Write data.
        dataStartOffset = bitStream.BitIndex;
    }

    private static void Write_End<TStreamType>(ref BitStream<TStreamType> bitStream, int idFromSerializer, int? id, int dataStartOffset, int frameOffset) where TStreamType : IByteStream
    {
        var dataEndOffset = bitStream.BitIndex;

        // Override ID if needed.
        if (id.HasValue)
            idFromSerializer = id.Value;

        // Create header.
        var header = new FrameGeneric
        {
            Version = CurrentVersion,
            SizeInBits = dataEndOffset - dataStartOffset,
            Id = idFromSerializer
        };

        // Write frame header.
        bitStream.BitIndex = frameOffset;
        Write(ref bitStream, ref header);
        bitStream.BitIndex = dataEndOffset;
    }

    /// <summary>
    /// Delegate used for serializing a given data type.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TStreamType"></typeparam>
    /// <param name="bitStream">The stream to serialize to.</param>
    /// <param name="data">The data to be serialized.</param>
    /// <returns>Frame type for this data. <see cref="FrameGeneric.Id"/></returns>
    public delegate int WriteFrameData<TData, TStreamType>(ref BitStream<TStreamType> bitStream, ref TData data) where TStreamType : IByteStream;

    /// <summary>
    /// Delegate used for serializing a given data type.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TStreamType"></typeparam>
    /// <param name="bitStream">The stream to serialize to.</param>
    /// <param name="data">The data to be serialized.</param>
    /// <returns>Frame type for this data. <see cref="FrameGeneric.Id"/></returns>
    public delegate int WriteFrameDataSpan<TData, TStreamType>(ref BitStream<TStreamType> bitStream, ref Span<TData> data) where TStreamType : IByteStream;
}