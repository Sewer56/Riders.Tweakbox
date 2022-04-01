using System;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary.Serializer.Physics;

public struct GenericSpanSerializer
{
    /// <summary>
    /// Node ID.
    /// </summary>
    public const int Id = GenericSerializer.Id;

    public const int IdCharaTypeStats = 0x54535453; // TSTS
    public const int IdTurb = 0x54555242; // TURB

    public static int Serialize<TByteStream, TData>(ref BitStream<TByteStream> bitStream, ref Span<TData> data) where TByteStream : IByteStream where TData : unmanaged
    {
        for (int x = 0; x < data.Length; x++)
            bitStream.WriteGeneric(ref data[x]);

        return Id;
    }

    public static void Deserialize<TByteStream, TData>(ref BitStream<TByteStream> bitStream, Span<TData> data) where TByteStream : IByteStream where TData : unmanaged
    {
        for (int x = 0; x < data.Length; x++)
            bitStream.ReadGeneric(out data[x]);
    }

    public static void DeserializeToNewArray<TByteStream, TData>(ref BitStream<TByteStream> bitStream, ref TData[] data, int numItems) where TByteStream : IByteStream where TData : unmanaged
    {
        data = new TData[numItems];
        Deserialize(ref bitStream, data.AsSpan());
    }
}