using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Voat.Common
{
    public class MatchMaker
    {
        private List<Match> _allMatches = new List<Match>();
        private List<Match> _filtered = null;

        public bool IgnoreDuplicateMatches { get; set; } = false;

        public int MatchThreshold { get; set; } = 0;

        public Func<Match, IEnumerable<Match>, bool> IsDuplicate { get; set; } = (match, matches) => matches.Any(x => x.Value == match.Value && x.Index != match.Index);

        public IEnumerable<Match> Matches
        {
            get
            {
                return _allMatches;
            }
        }

        public IEnumerable<Match> FilteredMatches
        {
            get
            {
                if (_filtered == null)
                {
                    _filtered = new List<Match>();

                    if (_allMatches.Any())
                    {
                        for (int i = 0; i < _allMatches.Count; i++)
                        {
                            var match = _allMatches[i];

                            //Really simple duplicate detection
                            if (!IgnoreDuplicateMatches || (IgnoreDuplicateMatches && !IsDuplicate(match, _filtered)))
                            {
                                if (MatchThreshold == 0 || _filtered.Count < MatchThreshold)
                                {
                                    _filtered.Add(match);
                                }
                            }
                        }
                    }
                }
                return _filtered;
            }
        }

        public bool Process(string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            if (input != null)
            {
                var matches = Regex.Matches(input, pattern, options);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        _allMatches.Add(match);
                    }
                }
            }
            return _allMatches.Any();
        }
    }
}
