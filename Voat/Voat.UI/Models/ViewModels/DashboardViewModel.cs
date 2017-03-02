using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class DashboardViewModel
    {

        public IDictionary<DomainType, IEnumerable<string>> Subscriptions { get; set; }

        public IEnumerable<DomainReference> TopBar { get; set; }
    }
}