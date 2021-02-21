using MessagePack;
using MessagePack.Formatters;
using Sewer56.Imgui.Controls;

namespace Riders.Tweakbox.Definitions.Serializers
{
    public class TextInputDataFormatter : IMessagePackFormatter<TextInputData>
    {
        /// <inheritdoc />
        public void Serialize(ref MessagePackWriter writer, TextInputData value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Text);
        }

        /// <inheritdoc />
        public TextInputData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new TextInputData(reader.ReadString(), 64);
        }
    }
}
