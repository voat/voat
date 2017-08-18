#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
