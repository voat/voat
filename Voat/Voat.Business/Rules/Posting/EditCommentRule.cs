using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment edit if data is acceptable.", "approved = (comment.IsValid())")]
    public class EditCommentRule : BaseCommentRule
    {
        public EditCommentRule()
            : base("Edit Comment", "7.6", RuleScope.EditComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            Data.Models.Comment comment = context.PropertyBag.Comment;

            if (comment.IsDeleted)
            {
                return base.CreateOutcome(RuleResult.Denied, "Deleted comments can not be edited");
            }
            if (comment.UserName != context.UserName)
            {
                return base.CreateOutcome(RuleResult.Denied, "User doesn't have permissions to perform requested action");
            }
            //rules checkd in base class
            return base.EvaluateRule(context);
        }
    }

}
