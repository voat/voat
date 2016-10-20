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
