using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Models
{
    public class Vote
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SubmissionID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ShowCurrentStats { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }

        public List<VoteOption> Options { get; set; }
        //public List<VoteRestriction> Restrictions { get; set; }

    }
    public class VoteOption
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SortOrder { get; set; }

        //public List<VoteOutcome> VoteOutcomes { get; set; }
    }
}
