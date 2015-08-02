using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Voat.Models.Api.v1 {
    public class ApiUserPreferences {

        //[JsonProperty("userName")]
        //[DataMember(Name = "userName")]
        //public string UserName { get; set; }
        
        [JsonProperty("disableCustomCSS")]
        [DataMember(Name = "disableCustomCSS")]
        public bool DisableCustomCSS { get; set; }

        [JsonProperty("enableNightMode")]
        [DataMember(Name = "enableNightMode")]
        public bool EnableNightMode { get; set; }

        [JsonProperty("language")]
        [DataMember(Name = "language")]
        public string Language { get; set; }

        [JsonProperty("openLinksNewWindow")]
        [DataMember(Name = "openLinksNewWindow")]
        public bool ClickingMode { get; set; }

        [JsonProperty("enableAdultContent")]
        [DataMember(Name = "enableAdultContent")]
        public bool EnableAdultContent { get; set; }


        [JsonProperty("publiclyDisplayVotes")]
        [DataMember(Name = "publiclyDisplayVotes")]
        public bool PubliclyDisplayVotes { get; set; }

        [JsonProperty("publiclyDisplaySubscriptions")]
        [DataMember(Name = "publiclyDisplaySubscriptions")]
        public bool PubliclyShowSubscriptions { get; set; }
        
    }
}