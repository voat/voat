using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common
{
    public class FlagEnumJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> values = new List<string>();
            string value = null;
            while((value = reader.ReadAsString()) != null)
            {
                values.Add(value);
            } 
            var result = Enum.Parse(objectType, String.Join(',', values.ToArray()));

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueString = value.ToString();
            var values = valueString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            writer.WriteStartArray();
            foreach (string enumValue in values)
            {
                writer.WriteValue(enumValue);
            }
            writer.WriteEndArray();
        }
    }
}
