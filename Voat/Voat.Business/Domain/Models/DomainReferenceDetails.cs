using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{

    public class FeaturedDomainReferenceDetails : DomainReferenceDetails
    {
        public DateTime FeaturedDate { get; set; }
        public string FeaturedBy { get; set; }

    }

    public class DomainReferenceDetails : DomainReference
    {

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public int SubscriberCount { get; set; }

        public dynamic Object { get; set; }

        public static DomainReferenceDetails Map(Data.Models.Subverse subverse)
        {
            var d = new DomainReferenceDetails();
            d.Object = subverse;
            d.Type = DomainType.Subverse;
            d.Description = subverse.Description;
            d.Name = subverse.Name;
            d.Title = subverse.Title;
            d.OwnerName = subverse.CreatedBy;
            d.SubscriberCount = subverse.SubscriberCount.Value;
            d.CreationDate = subverse.CreationDate;
            return d;
        }
        public static DomainReferenceDetails Map(Domain.Models.Set set)
        {
            var d = new DomainReferenceDetails();
            d.Object = set;
            d.Type = DomainType.Set;
            d.Description = set.Description;
            d.Name = set.Name;
            d.Title = set.Title;
            d.OwnerName = set.UserName;
            d.SubscriberCount = set.SubscriberCount;
            d.CreationDate = set.CreationDate;
            return d;
        }
    }
}
