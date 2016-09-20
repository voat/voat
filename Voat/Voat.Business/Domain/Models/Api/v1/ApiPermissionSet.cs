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
    }
}
