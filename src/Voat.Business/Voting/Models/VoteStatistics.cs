using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Data;

namespace Voat.Voting.Models
{
    public enum VoteRestrictionStatus
    {
        Certified,
        Uncertified,
        All
    }
    [DebuggerDisplay("Count: {Count}, Percentage: {Percentage}")]
    public class VoteSummary
    {
        public int Count { get; set; }
        public decimal Percentage {get; set;}
    }

    public class VoteBreakdowns : Dictionary<int, VoteSummary>
    {
        public VoteBreakdowns()
        {
        }
        public VoteBreakdowns(IEnumerable<KeyValuePair<int, VoteSummary>> values)
        {
            values.ForEach(x => Add(x.Key, x.Value));
        }
        public int TotalCount
        {
            get
            {
                return Values.Sum(x => x.Count);
            }
        }
    }

    public class VoteStatistics
    {
        private Dictionary<int, int> _allStatistics = null;

        private Dictionary<VoteRestrictionStatus, VoteBreakdowns> _friendlyStatistics;

        public int VoteID { get; set; }

        /// <summary>
        /// Resets calculated values. Call this method is Raw dictionary values change
        /// </summary>
        public void Reset()
        {
            _allStatistics = null;
            _friendlyStatistics = null;
        }

        public Dictionary<VoteRestrictionStatus, Dictionary<int, int>> Raw { get; set; } = new Dictionary<VoteRestrictionStatus, Dictionary<int, int>>();

        [JsonIgnore]
        public Dictionary<VoteRestrictionStatus, VoteBreakdowns> Friendly
        {
            get
            {
                if (_friendlyStatistics == null)
                {
                    _friendlyStatistics =
                        All
                        .GroupBy(x => x.Key)
                        .ToDictionary(
                            x => x.Key,
                            y => new VoteBreakdowns(y.SelectMany(x =>
                                x.Value.Select(d =>
                                    KeyValuePair.Create(
                                        d.Key,
                                        new VoteSummary()
                                        {
                                            Count = d.Value,
                                            Percentage = d.Value / (decimal)x.Value.Sum(s => s.Value)
                                        })
                               )))
                         );

                    //if (!_friendlyStatistics.ContainsKey(VoteRestrictionStatus.Certified))
                    //{
                    //    _friendlyStatistics.Add(VoteRestrictionStatus.Certified, new VoteBreakdowns());
                    //}
                    //if (!_friendlyStatistics.ContainsKey(VoteRestrictionStatus.Uncertified))
                    //{
                    //    _friendlyStatistics.Add(VoteRestrictionStatus.Uncertified, new VoteBreakdowns());
                    //}

                }
                return _friendlyStatistics;
            }
            set
            {
                _friendlyStatistics = value;
            }
        }

        public DateTime CreationDate { get; set; } = Repository.CurrentDate;

        [JsonIgnore]
        public Dictionary<VoteRestrictionStatus, Dictionary<int, int>> All {
            get
            {
                var stats = Raw;

                if (_allStatistics == null && Raw.Count > 1)
                {
                    var rawStats = Raw;
                    if (rawStats.ContainsKey(VoteRestrictionStatus.Certified) && rawStats.ContainsKey(VoteRestrictionStatus.Uncertified))
                    {
                        //Join then
                        var failed = Raw[VoteRestrictionStatus.Uncertified];
                        var passed = Raw[VoteRestrictionStatus.Certified];

                        var keys = failed.Keys.Union(passed.Keys);
                        var summedold = keys.ToDictionary(key => key, key => (passed.Keys.Contains(key) ? passed[key] : 0) + (failed.Keys.Contains(key) ? failed[key] : 0));

                        _allStatistics = (from e in passed.Concat(failed)
                                              group e by e.Key into g
                                              select new { Key = g.Key, Value = g.Sum(x => x.Value) })
                                  .ToDictionary(k => k.Key, v => v.Value);
                    }
                    else if (rawStats.ContainsKey(VoteRestrictionStatus.Certified))
                    {
                        _allStatistics = rawStats[VoteRestrictionStatus.Certified];
                    }
                    else if (rawStats.ContainsKey(VoteRestrictionStatus.Uncertified))
                    {
                        _allStatistics = rawStats[VoteRestrictionStatus.Uncertified];
                    }
                    else
                    {
                        _allStatistics = new Dictionary<int, int>();
                    }
                }

                if (_allStatistics != null)
                {
                    stats = Raw.Concat(KeyValuePair.Create(VoteRestrictionStatus.All, _allStatistics).ToEnumerable()).ToDictionary(x => x.Key, y => y.Value);
                }

                return stats;
            }
        }
    }
}
