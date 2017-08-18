using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Data.Models
{
    public class VoteOption
    {
        public int ID { get; set; }
        public int VoteID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SortOrder { get; set; }
     
        public List<VoteOutcome> VoteOutcomes { get; set; }
    }
}
