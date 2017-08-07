using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Voat.Common;

namespace Voat.Voting.Restrictions
{
    public class VoteRestrictionSet
    {
        private Dictionary<string, List<IVoteRestriction>> _restrictionSet = new Dictionary<string, List<IVoteRestriction>>();

        public void Populate(IEnumerable<Data.Models.VoteRestriction> restrictions)
        {
            throw new NotImplementedException();
            //foreach (var restriction in restrictions)
            //{
            //    var groupName = String.IsNullOrEmpty(restriction.Options.TrimSafe()) ? "" : restriction.GroupName.TrimSafe();

            //    List<IVoteRestriction> groupList = null;
            //    if (_restrictionSet.ContainsKey(groupName))
            //    {
            //        groupList = _restrictionSet[groupName];
            //    }
            //    else
            //    {
            //        groupList = new List<IVoteRestriction>();
            //        _restrictionSet[groupName] = groupList;
            //    }
            //    groupList.Add((IVoteRestriction)OptionHandler.Construct(restriction.Type, restriction.Options));
            //}
        }
        public RestrictionEvaluation Evaluate(IPrincipal user)
        {
            var violations = new RestrictionEvaluation();
            if (_restrictionSet.Count == 0)
            {
                return violations;
            }
            foreach (string group in _restrictionSet.Keys)
            {
                foreach (var restriction in _restrictionSet[group])
                {
                    var v = restriction.Evaluate(user);
                    if (!v.Success)
                    {
                        violations.Violations.Add(v);
                    }
                }
                if (violations.IsValid)
                {
                    break;
                }
            }
            return violations;
        }
    }
}
