using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Voat.Domain.Models
{
    public class SendMessage
    {
        private string _sender = null;

        [Required]
        public string Message { get; set; }

        [Required]
        public string Recipient { get; set; }

        [JsonIgnore]
        public string Sender
        {
            get
            {
                if (string.IsNullOrEmpty(_sender) && System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    return System.Threading.Thread.CurrentPrincipal.Identity.Name;
                }
                return _sender;
            }

            set
            {
                _sender = value;
            }
        }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }
    }
}
