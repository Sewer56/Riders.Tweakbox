using System;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Tweakbox.Definitions.Serializers.Binary.Serializer.Physics;

public struct DecelPropertiesSerializer
{
    /// <summary>
    /// Node ID.
    /// </summary>
    public const int Id = 0x4445434C; // DECL

    public static int Serialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref DecelProperties data) where TByteStream : IByteStream
    {
        bitStream.WriteGeneric(data.Mode);
        bitStream.WriteGeneric(data.LinearSpeedCapOverride);
        bitStream.WriteGeneric(data.LinearMaxSpeedOverCap);
        bitStream.Write(Convert.ToByte(data.EnableMaxSpeedOverCap));
        return Id;
    }
    public static void Deserialize<TByteStream>(ref BitStream<TByteStream> bitStream, ref DecelProperties data) where TByteStream : IByteStream
    {
        data.Mode = bitStream.ReadGeneric<DecelMode>();
        data.LinearSpeedCapOverride = bitStream.ReadGeneric<float>();
        data.LinearMaxSpeedOverCap = bitStream.ReadGeneric<float>();
        data.EnableMaxSpeedOverCap = Convert.ToBoolean(bitStream.Read<byte>());
    }
}