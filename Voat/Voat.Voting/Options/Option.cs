using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Configuration;

namespace Voat.Voting.Options
{
    public abstract class Option
    {
        public static T Deserialize<T>(string json) where T: Option
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings.FriendlySerializationSettings);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, JsonSettings.FriendlySerializationSettings);
        }
    }
}