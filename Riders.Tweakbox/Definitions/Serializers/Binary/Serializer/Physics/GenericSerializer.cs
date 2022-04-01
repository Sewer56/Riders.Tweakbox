using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary.Serializer.Physics;

public struct GenericSerializer
{
    /// <summary>
    /// Node ID.
    /// </summary>
    public const int Id = 0x554E4B4E; // UNKN

    public const int IdRunningPhysics1 = 0x52554E31; // RUN1
    public const int IdRunningPhysics2 = 0x52554E32; // RUN2

    public static int Serialize<TByteStream, TData>(ref BitStream<TByteStream> bitStream, ref TData data) where TByteStream : IByteStream where TData : unmanaged
    {
        bitStream.WriteGeneric(ref data);
        return Id;
    }

    public static void Deserialize<TByteStream, TData>(ref BitStream<TByteStream> bitStream, ref TData data) where TByteStream : IByteStream where TData : unmanaged
    {
        bitStream.ReadGeneric(out data);
    }
}