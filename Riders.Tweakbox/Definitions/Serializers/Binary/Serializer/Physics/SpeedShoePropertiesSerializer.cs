using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary.Serializer.Physics;

public struct SpeedShoePropertiesSerializer
{
    /// <summary>
    /// Node ID.
    /// </summary>
    public const int Id = 0x53505348; // SPSH

    public static int Serialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref SpeedShoeProperties data) where TByteStream : IByteStream
    {
        bitStream.WriteGeneric(data.Mode);
        bitStream.WriteGeneric(data.FixedSpeed);
        bitStream.WriteGeneric(data.AdditiveSpeed);
        bitStream.WriteGeneric(data.MultiplicativeSpeed);
        bitStream.WriteGeneric(data.MultiplicativeMinSpeed);
        return Id;
    }
    public static void Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref SpeedShoeProperties data) where TByteStream : IByteStream
    {
        data.Mode = bitStream.ReadGeneric<SpeedShoesMode>();
        data.FixedSpeed = bitStream.ReadGeneric<float>();
        data.AdditiveSpeed = bitStream.ReadGeneric<float>();
        data.MultiplicativeSpeed = bitStream.ReadGeneric<float>();
        data.MultiplicativeMinSpeed = bitStream.ReadGeneric<float>();
    }
}