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
        bitStream.WriteGeneric(data.FixedSpeed);
        bitStream.WriteGeneric(data.AdditiveSpeed);
        bitStream.WriteGeneric(data.MultiplicativeSpeed);
        bitStream.WriteGeneric(data.MultiplicativeMinSpeed);
        return Id;
    }

    public static void Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref DashPanelProperties data, int sizeInBits) where TByteStream : IByteStream
    {
        // Sanity Check.
        if (sizeInBits <= 4 * 8)
            return;

        bitStream.ReadGeneric(out data.Mode);
        data.FixedSpeed = bitStream.ReadGeneric<float>();
        data.AdditiveSpeed = bitStream.ReadGeneric<float>();
        data.MultiplicativeSpeed = bitStream.ReadGeneric<float>();
        data.MultiplicativeMinSpeed = bitStream.ReadGeneric<float>();

        // Version 1. 20 Bytes.
        if (sizeInBits <= 20 * 8)
            return;
    }
}