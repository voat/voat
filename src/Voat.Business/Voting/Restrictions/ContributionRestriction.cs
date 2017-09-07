using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Domain.Models;

namespace Voat.Voting.Restrictions
{
    [Flags]
    public enum ContentTypeRestriction
    {
        Submission = 1,
        Comment = 2,
        Any = Submission | Comment
    }
    public abstract class ContributionRestriction : VoteRestriction
    {
        [Required]
        [DisplayName("Type")]
        //[JsonConverter(typeof(FlagEnumJsonConverter))]
        public ContentTypeRestriction ContentType { get; set; }
        [Required]
        [DisplayName("Minimum Count")]
        public int MinimumCount { get; set; }
        [DisplayName("Maximum Count")]
        public int? MaximumCount { get; set; }

        public string Subverse { get; set; }

        #region Formatting
        protected string ContentTypeDescription(bool usePlural = false)
        {
            var flags = ContentType.GetEnumFlags();
            var content = String.Join(" or ", flags.Select(x => x.ToString() + (usePlural ? "s" : "")));
            return content;
        }
        protected string WhereDescription()
        {
            var where = $"to {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Subverse))
            {
                where = $"in v/{Subverse}";
            }
            return where;
        }
        protected string DateRangeDescription()
        {
            //will need to modify this
            return DateRange.ToString();
            
        }
        #endregion

    }
}
