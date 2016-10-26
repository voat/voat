using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    /// <summary>
    /// Represents a single comment including necessary submission fields
    /// </summary>
    public class SubmissionComment : Comment
    {
        public SubmissionSummary Submission { get; set; } = new SubmissionSummary(); 

    }
    public class SubmissionSummary
    {
        public string Title { get; set; }

        public string UserName { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAnonymized { get; set; }
    }
}
