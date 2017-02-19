using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{

    public enum MenuType
    {
        Subverse,
        Set,
        Domain,
        UserProfile,
        SubverseDiscovery,
        UserMessages,
        Smail
    }

    public class NavigationViewModel
    {
        public MenuType MenuType { get; set; }

        public string BasePath { get; set; }

        public SortAlgorithm? Sort { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
}