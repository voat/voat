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

        private Domain.Models.SortAlgorithm? _sort = null;
        private Domain.Models.SortSpan? _span = null;


        public bool PreviewMode { get; set; }
        public string Title { get; set; }
        //public string UrlAction { get; set; }

        public Domain.Models.DomainReference Context { get; set; }

        public Domain.Models.SortAlgorithm? Sort {
            get
            {
                return _sort;
            }
            set
            {
                if (value == Domain.Models.SortAlgorithm.Rank)
                {
                    value = null;
                }
                _sort = value;
            }
        }
        public Domain.Models.SortSpan? Span
        {
            get
            {
                return _span;
            }
            set
            {
                if (value == Domain.Models.SortSpan.All)
                {
                    value = null;
                }
                _span = value;
            }
        }
        public PaginatedList<Domain.Models.Submission> Submissions { get; set; }

        public bool IsActualSubverse
        {
            get
            {
                if (Context.Type == Domain.Models.DomainType.Subverse)
                {
                    var subverse = Context.Name;
                    return !(String.IsNullOrEmpty(subverse) || subverse.IsEqual("user") || subverse.IsEqual("all") || AGGREGATE_SUBVERSE.IsAggregate(subverse));

                }
                return false;
            }
        }
    }
}