using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Business.DataAccess.Commands;
using Voat.Rules;
using Voat.RulesEngine;

namespace Voat.Business.DataAccess.Handlers
{
    public class VoteCommandHandler : CommandHandler,
        ICommandHandler<SubmissionVoteCommand, CommandResponse>, 
        ICommandHandler<CommentVoteCommand, CommandResponse>
    {

        private CommandResponse Map(RuleOutcome outcome)
        {
            return CommandResponse.Denied(outcome.ToString(), outcome.Message);
        }

        public CommandResponse Execute(CommentVoteCommand command)
        {
            var outcome = VoatRulesEngine.Instance.IsCommentVoteAllowed(command.CommentID, command.VoteValue);
            if (outcome.IsAllowed)
            {
                //Perform vote
                return CommandResponse.Success();
            }
            else
            {
                return Map(outcome);
            }
        }

        public CommandResponse Execute(SubmissionVoteCommand commandType)
        {
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RulesEngine.RuleScope.Vote, true);
            if (outcome.IsAllowed)
            {
                //Perform vote
                return CommandResponse.Success();
            }
            else
            {
                return Map(outcome);
            }
        }
    }

}
