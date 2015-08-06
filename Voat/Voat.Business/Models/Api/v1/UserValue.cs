
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Voat.Utilities;

namespace Voat.Models.Api.v1
{

    [DataContract]
    public class UserValue {

        public UserValue() {
            /*no-op*/
        }
        public UserValue(string value) {
            this.Value = value;
        }

        [IgnoreDataMember]
        [JsonIgnore]
        public bool IsValid {
            get {
                return !String.IsNullOrEmpty(this.Value);
            }
        }

        /// <summary>
        /// Content of request
        /// </summary>
        [RequiredAttribute]
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Value { get; set; }

    }
}