using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Voat.Models.Api.v1 {
    public class ApiSubscription {
        private SubscriptionType _type = SubscriptionType.Subverse;

        

        
        /// <summary>
        /// Specifies the type of subscription. 
        /// </summary>
        [JsonProperty("type")]
        [DataMember(Name = "type")]
        public SubscriptionType Type {
            get {
                return _type;
            }

            set {
                _type = value;
            }
        }
        /// <summary>
        /// The friendly name of the subscription
        /// </summary>
        [JsonProperty("typeName")]
        [DataMember(Name = "typeName")]
        public string TypeName {
            get {
                return _type.ToString();
            }
        }

        /// <summary>
        /// Specifies the name of the subscription item.
        /// </summary>
        [JsonProperty("name")]
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}