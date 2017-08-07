using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Voat.Domain.Models;

namespace Voat.Voting.Options
{
    public class ContentOption : SubverseOption
    {
        [DisplayName("Type")]
        public ContentType ContentType { get; set; }
        [DisplayName("Minimum Count")]
        public int MinimumCount { get; set; }
        [DisplayName("Maximum Count")]
        public int? MaximumCount { get; set; }
    }
}
