using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Configuration;

namespace Voat.Voting
{
    public abstract class VoteItem : IDescription
    {
        public int ID { get; set; }

        public abstract string ToDescription();
        public static T Deserialize<T>(string json) where T : VoteItem
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings.DataSerializationSettings);
        }
        public string TypeName
        {
            get
            {
                return this.GetType().Name;
            }
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, JsonSettings.DataSerializationSettings);
        }
    }
}
