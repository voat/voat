using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Voat.Domain.Models
{
    public class SendMessage
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public string Recipient { get; set; }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }
    }
}
