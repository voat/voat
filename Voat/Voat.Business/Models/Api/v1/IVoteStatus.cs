using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Models.Api.v1
{
    public interface IVoteStatus
    {

        [JsonProperty("voteStatus")]
        [DataMember(Name = "voteStatus")]
        int? VoteStatus { get; }
    }
}
