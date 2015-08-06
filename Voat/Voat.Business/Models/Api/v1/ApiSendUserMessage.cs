using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Voat.Models.Api.v1 {

    public class ApiSendUserMessage {

        [RequiredAttribute]
        [JsonProperty("recipient")]
        [DataMember(Name = "recipient")]
        public string Recipient { get; set; }

        [Required]
        [MaxLength(50)]
        [JsonProperty("subject")]
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        [Required]
        [JsonProperty("message")]
        [DataMember(Name = "message")]
        public string Message { get; set; }

    }
}