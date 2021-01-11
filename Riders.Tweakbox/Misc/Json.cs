using System;
using System.Text;
using System.Text.Json;

namespace Riders.Tweakbox.Misc
{
    public static class Json
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        /// <summary>
        /// Deserializes a struct, including the fields and comments.
        /// </summary>
        public static T DeserializeStruct<T>(Span<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data, _options);
        }

        /// <summary>
        /// Serializes a struct, including the fields and comment
        /// </summary>
        public static byte[] SerializeStruct<T>(ref T value)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(value, _options));
        }
    }
}
