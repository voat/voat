using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class SubverseSubscriptionDetail
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public int? SubscriberCount { get; set; }
        public DateTime SubscriptionDate { get; set; }
    }
}
