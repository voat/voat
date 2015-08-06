using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Voat.Utilities
{
    public class JsonUnprintableCharConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        public static string ProcessChars(string source)
        {

            if (String.IsNullOrEmpty(source))
            {
                return source;
            }

            List<char> chars = new List<char>();
            foreach (char c in source)
            {
                switch (c)
                {
                    //case '\a':
                    //    chars.Add('\\');
                    //    chars.Add('a');
                    //    break;
                    //case '\b':
                    //    chars.Add('\\');
                    //    chars.Add('b');
                    //    break;
                    //case '\f':
                    //    chars.Add('\\');
                    //    chars.Add('f');
                    //    break;
                    //case '\r':
                    //    //chars.Add('\\');
                    //    //chars.Add('r');
                    //    break;
                    //case '\t':
                    //    chars.Add('\\');
                    //    chars.Add('t');
                    //    break;
                    //case '\v':
                    //    chars.Add('\\');
                    //    chars.Add('v');
                    //    break;
                    case '\a':
                    case '\b':
                    case '\f':
                    case '\r':
                    case '\t':
                    case '\v':
                        /*ignore these*/
                        break;
                    case '\n': //add another so it breaks in Markdown.
                        chars.Add('\n');
                        chars.Add('\n');
                        break;

                    default:
                        chars.Add(c);
                        break;
                }

            }

            return new string(chars.ToArray());
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                return ProcessChars(reader.Value.ToString());
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}