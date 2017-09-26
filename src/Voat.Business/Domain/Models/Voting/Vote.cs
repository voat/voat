using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Validation;
using Voat.Voting.Attributes;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

namespace Voat.Domain.Models
{
    public interface IIdentifier<T>
    {
        T ID { get; set; }
    }
    //testing out base type ID
    public class Identifier<T> : IIdentifier<T>
    {
        public T ID { get; set; }
    }
    public class CreateVote : Identifier<int>
    {
        //public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Subverse { get; set; }

        public List<CreateVoteOption> Options { get; set; } = new List<CreateVoteOption>();
        public List<CreateVoteType> Restrictions { get; set; } = new List<CreateVoteType>();

        public class CreateVoteOption : Identifier<int>
        {
            [Required]
            public string Title { get; set; }
            public string Content { get; set; }

            public List<CreateVoteType> Outcomes { get; set; } = new List<CreateVoteType>();
        }
        public class CreateVoteType : Identifier<int>
        {
            
            public string TypeName { get; set; }
            public object Options { get; set; }

            public T Construct<T>()
            {
                var metadata = VoteMetadata.Instance.FindByName(TypeName);
                var o = (T)JsonConvert.DeserializeObject(Options.ToString(), metadata.Type, JsonSettings.DataInputSerializationSettings);
                return o;
            }
        }
    }
    public class VoteMetadata
    {
        private static VoteMetadata _instance = null;
        private List<DiscoveredType<VoteAttribute>> _metaData = null;

        public List<DiscoveredType<VoteAttribute>> VoteData
        {
            get
            {
                if (_metaData == null)
                {
                    _metaData = TypeDiscovery.DiscoverTypes<VoteAttribute>(new[] { this.GetType().Assembly }, true).ToList();
                }
                return _metaData;
            }
        }
        public IEnumerable<DiscoveredType<VoteAttribute>> Restrictions
        {
            get
            {
                return VoteData.Where(x => x.Type.IsSubclassOf(typeof(VoteRestriction)));
            }
        }
        public IEnumerable<DiscoveredType<VoteAttribute>> Outcomes
        {
            get
            {
                return VoteData.Where(x => x.Type.IsSubclassOf(typeof(VoteOutcome)));
            }
        }
        public DiscoveredType<VoteAttribute> FindByName(string name)
        {
            var result = VoteData.FirstOrDefault(x => x.Type.Name.IsEqual(name));
            return result;
        }
        public DiscoveredType<VoteAttribute> FindByType(Type type)
        {
            var result = VoteData.FirstOrDefault(x => x.Type == type);
            return result;
        }
        public static VoteMetadata Instance
        {
            get
            {
                if (_instance == null) { _instance = new VoteMetadata(); }
                return _instance;
            }
        }
    }

    

    //[DataValidation(typeof(VoteModelValidator), "", "", 1)]
    public class Vote : IValidatableObject
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "The title must be at least 2 and no more than 200 characters long", MinimumLength = 2)]
        public string Title { get; set; }

        [MaxLength(10000, ErrorMessage = "Content is limited to 10,000 characters")]
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SubmissionID { get; set; }
        [Required]
        public string Subverse { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool DisplayStatistics { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }

        [PerformValidation]
        public List<VoteOption> Options { get; set; } = new List<VoteOption>();

        [PerformValidation]
        public List<VoteRestriction> Restrictions { get; set; } = new List<VoteRestriction>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (Options == null || Options.Count < 2)
            {
                errors.Add(ValidationPathResult.Create(this, "A Vote must have at least 2 options", (m) => m.Options));
            }
            if (Options.Count > 10)
            {
                errors.Add(ValidationPathResult.Create(this, "A Vote is limited to 10 options", (m) => m.Options));
            }

            //Ensure no duplicate titles
            if (Options != null || Options.Count >= 2)
            {
                var grouped = Options.GroupBy(x => x.Title.TrimSafe().ToNormalized(Normalization.Lower)).Select(x => new { Key = x.Key, Count = x.Count() });
                var duplicateTitle = grouped.FirstOrDefault(x => x.Count > 1);
                if (duplicateTitle != null)
                {
                    errors.Add(ValidationPathResult.Create(this, "Option titles must be unique.", (m) => m.Options));
                }
            }

            return errors;
        }
    }
    public class VoteOption
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "The title must be at least 2 and no more than 200 characters long", MinimumLength = 2)]
        public string Title { get; set; }

        [MaxLength(10000, ErrorMessage = "Content is limited to 10,000 characters")]
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SortOrder { get; set; }

        [PerformValidation]
        public List<VoteOutcome> Outcomes { get; set; } = new List<VoteOutcome>();
    }
}
