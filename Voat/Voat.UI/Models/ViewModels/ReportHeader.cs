using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class ReportContentModel
    {
        public string Subverse { get; set; }
        public int ID { get; set; }
        public ContentType ContentType { get; set; }
        [Required(ErrorMessage = "So... You want us to guess? Ok. How about 3?")]
        public int? RuleSetID { get; set; }

        public IEnumerable<RuleSet> Rules { get; set; }
    }
}