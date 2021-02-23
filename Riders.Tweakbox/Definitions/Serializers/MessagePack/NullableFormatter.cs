using MessagePack;
using MessagePack.Formatters;

namespace Riders.Tweakbox.Definitions.Serializers.MessagePack
{
    public unsafe class NullableFormatter<T> : IMessagePackFormatter<Nullable<T>> where T : unmanaged
    {
        /// <inheritdoc />
        public void Serialize(ref MessagePackWriter writer, Nullable<T> value, MessagePackSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNil();
            }
            else
            {
                var formatter = options.Resolver.GetFormatter<T>();
                formatter.Serialize(ref writer, value.Value, options);
            }
        }

        /// <inheritdoc />
        public Nullable<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            // If not Binary/Nullable type, fall back to reading primitives.
            if (reader.TryReadNil())
                return new Nullable<T>();

            var formatter = options.Resolver.GetFormatter<T>();
            return formatter.Deserialize(ref reader, options);
        }
    }
}