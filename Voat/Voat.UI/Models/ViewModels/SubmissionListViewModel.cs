using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Data;
using Voat.Utilities;

namespace Voat.Models.ViewModels
{
    public class SubmissionListViewModel
    {
        public bool PreviewMode { get; set; }
        public string Title { get; set; }
        public string Subverse { get; set; }
        public Domain.Models.SortAlgorithm? Sort { get; set; }
        public Domain.Models.SortSpan? Span { get; set; }

        public PaginatedList<Data.Models.Submission> Submissions { get; set; }

        public bool IsActualSubverse
        {
            get
            {
                return !(Subverse.IsEqual("user") || Subverse.IsEqual("all") || AGGREGATE_SUBVERSE.IsAggregate(Subverse));
            }
        }
    }
}