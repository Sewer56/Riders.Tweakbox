using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary.Serializer.Physics;

public struct DashPanelPropertiesSerializer
{
    /// <summary>
    /// Node ID.
    /// </summary>
    public const int Id = 0x44504E4C; // DPNL

    public static int Serialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref DashPanelProperties data) where TByteStream : IByteStream
    {
        bitStream.WriteGeneric(data.Mode);
        bitStream.Write(data.FixedSpeed);
        bitStream.Write(data.AdditiveSpeed);
        bitStream.Write(data.MultiplicativeSpeed);
        bitStream.Write(data.MultiplicativeMinSpeed);
        return Id;
    }

    public static void Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref DashPanelProperties data) where TByteStream : IByteStream
    {
        bitStream.ReadGeneric(out data.Mode);
        data.FixedSpeed = bitStream.Read<float>();
        data.AdditiveSpeed = bitStream.Read<float>();
        data.MultiplicativeSpeed = bitStream.Read<float>();
        data.MultiplicativeMinSpeed = bitStream.Read<float>();
    }
}