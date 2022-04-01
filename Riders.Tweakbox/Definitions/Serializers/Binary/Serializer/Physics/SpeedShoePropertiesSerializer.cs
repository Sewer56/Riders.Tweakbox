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
        bitStream.Write(data.FixedSpeed);
        bitStream.Write(data.AdditiveSpeed);
        bitStream.Write(data.MultiplicativeSpeed);
        bitStream.Write(data.MultiplicativeMinSpeed);
        return Id;
    }
    public static void Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref SpeedShoeProperties data) where TByteStream : IByteStream
    {
        data.Mode = bitStream.ReadGeneric<SpeedShoesMode>();
        data.FixedSpeed = bitStream.Read<float>();
        data.AdditiveSpeed = bitStream.Read<float>();
        data.MultiplicativeSpeed = bitStream.Read<float>();
        data.MultiplicativeMinSpeed = bitStream.Read<float>();
    }
}