using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Models
{
    public class Ban
    {
        public string Subverse { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
