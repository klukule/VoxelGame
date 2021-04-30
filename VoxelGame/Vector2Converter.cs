using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Vector2 ret = Vector2.Zero;

            reader.Read();
            ret.X = ((float?)reader.ReadAsDouble()).Value;
            reader.Read();
            ret.Y = ((float?)reader.ReadAsDouble()).Value;
            reader.Read();
            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector2)value;
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(v.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(v.Y);
            writer.WriteEndObject();
        }
    }
}
