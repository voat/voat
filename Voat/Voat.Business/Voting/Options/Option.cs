using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Configuration;

namespace Voat.Voting.Options
{
    public abstract class Option
    {
        public static T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings.FriendlySerializationSettings);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, JsonSettings.FriendlySerializationSettings);
        }
    }
}