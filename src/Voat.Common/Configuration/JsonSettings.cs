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
using Voat.Common;

namespace Voat.Configuration
{
    public static class JsonSettings
    {
        private static JsonSerializerSettings _public = null;
        private static JsonSerializerSettings _data = null;
        private static JsonSerializerSettings _publicinput = null;
        private static JsonSerializerSettings _api = null;
        /// <summary>
        /// Settings to serialize the input and output of standardized Json 
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings FriendlySerializationSettings
        {
            get
            {
                var settings = Voat.Common.Extensions.GetOrLoad(ref _public, () => {
                    return ConfigureJsonSerializer(new JsonSerializerSettings());
                });
                return settings;
            }
        }
        /// <summary>
        /// Settings to serialize strongly typeed objects to and from Json including type names and field information 
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings DataSerializationSettings
        {
            get
            {
                var settings = Voat.Common.Extensions.GetOrLoad(ref _data, () => {
                    return new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };
                });
                return settings;
            }
        }
        public static JsonSerializerSettings DataInputSerializationSettings
        {
            get
            {
                var settings = Voat.Common.Extensions.GetOrLoad(ref _publicinput, () => {
                    return new JsonSerializerSettings()
                    {
                        //Leave errors alone, let validation handle
                        Error = (sender, args) => {
                            args.ErrorContext.Handled = true;
                        },
                        TypeNameHandling = TypeNameHandling.All,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };
                });
                return settings;
            }
        }
        public static JsonSerializerSettings APISerializationSettings
        {
            get
            {
                var settings = Voat.Common.Extensions.GetOrLoad(ref _api, () => {
                    var s = new JsonSerializerSettings();
                    s = ConfigureJsonSerializer(s);
                    //s.Converters.Add(new JsonUnprintableCharConverter());
                    return s;
                });
                return settings;
            }
        }
        public static JsonSerializerSettings ConfigureJsonSerializer(JsonSerializerSettings settings)
        {
            //camelCases all api output - no need for attributes
            //defaultJsonSerializer.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.Formatting = Formatting.None;

            //settings.Converters.Add(new JsonUnprintableCharConverter());
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}
