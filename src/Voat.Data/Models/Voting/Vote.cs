using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Data.Models
{
    public class Vote
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public string Subverse { get; set; }
        public int SubmissionID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public bool DisplayStatistics { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }

        public List<VoteOption> VoteOptions { get; set; }
        public List<VoteRestriction> VoteRestrictions { get; set; }

    }
}
