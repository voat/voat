using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class SetViewModel
    {
        public Data.Models.SubverseSet Set { get; set; }
        public Utilities.PaginatedList<Domain.Models.SubverseSubscriptionDetail> List { get; set; }
    }
}