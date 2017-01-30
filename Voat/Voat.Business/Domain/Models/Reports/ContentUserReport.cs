using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class ContentUserReport
    {
        public string Subverse { get; set; }
        public string UserName { get; set; }
        public int? SubmissionID { get; set; }
        public int? CommentID { get; set; }
        public int RuleSetID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public DateTime MostRecentReportDate { get; set; }
    }
    public class ContentItem
    {
        public ContentType ContentType { get; set; }
        public Domain.Models.Submission Submission { get; set; }
        public Domain.Models.Comment Comment { get; set; }

    }
}
