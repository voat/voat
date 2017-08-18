using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Voting.Attributes;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

namespace Voat.Domain.Models
{

    public class CreateVote
    {
        
        public string Title { get; set; }
        public string Content { get; set; }
        public string Subverse { get; set; }

        public List<CreateVoteOption> Options { get; set; } = new List<CreateVoteOption>();
        public List<CreateVoteType> Restrictions { get; set; } = new List<CreateVoteType>();

        public class CreateVoteOption
        {
            [Required]
            public string Title { get; set; }
            public string Content { get; set; }

            public List<CreateVoteType> Outcomes { get; set; } = new List<CreateVoteType>();
        }
        public class CreateVoteType 
        {
            
            public string Type { get; set; }
            public object Options { get; set; }

            public T Construct<T>()
            {
                var metadata = VoteMetadata.Instance.FindByName(Type);
                var o = (T)JsonConvert.DeserializeObject(Options.ToString(), metadata.Type, JsonSettings.DataSerializationSettings);
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

    public class Vote
    {
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SubmissionID { get; set; }
        [Required]
        public string Subverse { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ShowCurrentStats { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }

        public List<VoteOption> Options { get; set; } = new List<VoteOption>();
        public List<VoteRestriction> Restrictions { get; set; } = new List<VoteRestriction>();

    }
    public class VoteOption
    {
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }
        public int SortOrder { get; set; }

        public List<VoteOutcome> Outcomes { get; set; } = new List<VoteOutcome>();
    }
}
