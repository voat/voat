using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has more than 20 CCP", "approved = (user.CCP > 20)")]
    public class UpVoteCommentRule : BaseCCPVote
    {
        public UpVoteCommentRule() : base("UpVote Comment", "2.2", 20, RuleScope.UpVoteComment)
        {
        }
    }
}
