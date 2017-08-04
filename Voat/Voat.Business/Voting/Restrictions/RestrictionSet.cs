using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Voat.Voting.Restrictions
{
    public class VoteRestrictionSet
    {
        private Dictionary<string, List<IVoteRestriction>> _restrictionSet = new Dictionary<string, List<IVoteRestriction>>();

        public void Populate(IEnumerable<Data.Models.VoteRestriction> restrictions)
        {
            foreach (var restriction in restrictions)
            {
                List<IVoteRestriction> groupList = null;
                if (_restrictionSet.ContainsKey(restriction.GroupName))
                {
                    groupList = _restrictionSet[restriction.GroupName];
                }
                else
                {
                    groupList = new List<IVoteRestriction>();
                    _restrictionSet[restriction.GroupName] = groupList;
                }
                groupList.Add((IVoteRestriction)OptionHandler.Construct(restriction.Type, restriction.Options));
            }
        }
        public bool Evaluate(IPrincipal user)
        {
            if (_restrictionSet.Count == 0)
            {
                return true;
            }
            foreach (string group in _restrictionSet.Keys)
            {
                //evaluate all restrictions in group
                var result = _restrictionSet[group].All(x => x.Evaluate(user));
                if (result == true)
                {
                    return result;
                }
            }
            return false;
        }
    }
}
