using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame
{
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector3);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Vector3 ret = Vector3.Zero;

            reader.Read();
            ret.X = ((float?)reader.ReadAsDouble()).Value;
            reader.Read();
            ret.Y = ((float?)reader.ReadAsDouble()).Value;
            reader.Read();
            ret.Z = ((float?)reader.ReadAsDouble()).Value;
            reader.Read();
            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(v.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(v.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(v.Z);
            writer.WriteEndObject();
        }
    }
}
