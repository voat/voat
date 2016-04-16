using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserValue
    {
        public UserValue()
        {
            /*no-op*/
        }

        public UserValue(string value)
        {
            this.Value = value;
        }

        [IgnoreDataMember]
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return !String.IsNullOrEmpty(this.Value);
            }
        }

        /// <summary>
        /// Content of request
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
