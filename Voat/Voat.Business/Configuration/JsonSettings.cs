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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Voat.Configuration
{
    public static class JsonSettings
    {
        //TODO: Hack. Cache this or pull it from config
        public static JsonSerializerSettings GetSerializationSettings()
        {
            return ConfigureJsonSerializer(new JsonSerializerSettings());
        }

        public static JsonSerializerSettings ConfigureJsonSerializer(JsonSerializerSettings settings)
        {
            //camelCases all api output - no need for attributes
            //defaultJsonSerializer.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
            settings.Formatting = Newtonsoft.Json.Formatting.None;

            //settings.Converters.Add(new JsonUnprintableCharConverter());
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}
