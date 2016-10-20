using Newtonsoft.Json;

namespace Voat.Domain.Models
{
    public class ApiPermissionSet
    {
        [JsonProperty("allowStream")]
        public bool AllowStream { get; set; }

        [JsonProperty("allowUnrestrictedLogin")]
        public bool AllowUnrestrictedLogin { get; set; }

        [JsonProperty("requireHmacOnLogin")]
        public bool RequireHmacOnLogin { get; set; }

        //This property is being added to allow for read only keys
        [JsonProperty("allowLogin")]
        public bool AllowLogin { get; set; }
    }
}
