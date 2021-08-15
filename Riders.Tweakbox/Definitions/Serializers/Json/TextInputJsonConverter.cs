using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sewer56.Imgui.Controls;
namespace Riders.Tweakbox.Definitions.Serializers.Json;

public class TextInputJsonConverter : JsonConverter<TextInputData>
{
    /// <inheritdoc />
    public override TextInputData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new TextInputData(reader.GetString(), 128);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TextInputData value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Text);
    }
}
