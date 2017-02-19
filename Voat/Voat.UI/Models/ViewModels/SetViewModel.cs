using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class SetViewModel
    {
        public SetPermission Permissions { get; set; }
        public Domain.Models.Set Set { get; set; }
        public Utilities.PaginatedList<Domain.Models.SubverseSubscriptionDetail> List { get; set; }
    }
    
}