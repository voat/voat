#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Voat.Utilities
{
    public class JsonUnprintableCharConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
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

                    //case '\n': //add another so it breaks in Markdown.
                    //    chars.Add('\n');
                    //    chars.Add('\n');
                    //    break;

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
                var processed = ProcessChars(reader.Value.ToString());
                return processed;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
