using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Configuration;

namespace Voat.Voting
{
    public abstract class VoteItem : IDescription
    {
        public abstract string ToDescription();
        public static T Deserialize<T>(string json) where T : VoteItem
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings.DataSerializationSettings);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, JsonSettings.DataSerializationSettings);
        }
    }
}
