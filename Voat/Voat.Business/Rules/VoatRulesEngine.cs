using System;
using System.Diagnostics;
using Voat.RulesEngine;


namespace Voat.Rules
{

    public class VoatRulesEngine : RulesEngine<VoatRuleContext>
    {

        protected static VoatRulesEngine _engine;


        protected VoatRulesEngine(IRuleContextHandler<VoatRuleContext> handler)
            : base(handler)
        {

        }

        public static VoatRulesEngine Instance
        {
            get
            {
                if (_engine == null)
                {
                    lock (typeof(VoatRulesEngine))
                    {
                        if (_engine == null)
                        {
                            _engine = new VoatRulesEngine(new RequestContextHandler());
                            _engine.LoadDiscoverableRules(System.Reflection.Assembly.GetExecutingAssembly());
                        }
                    }
                }
                return _engine;
            }
            set
            {
                _engine = value;
            }
        }
        public override RuleOutcome EvaluateRuleSet(RuleScope scope, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {

            Debug.Print("~~~~~ RULE SET EVAL ~~~~~~~");
            Debug.Print("Scope: {0}", scope.ToString());
            Debug.Print("PRE EVAL CONTEXT ----------");
            Debug.Print(Context.ToString());
            Debug.Print("-");
            var outcome = base.EvaluateRuleSet(scope, includeGlobalScope, scopeEvaluator);
            Debug.Print("POST EVAL CONTEXT  --------");
            Debug.Print(Context.ToString());
            Debug.Print("Outcome: {0}", outcome.Result.ToString());
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Debug.Print("");

            return outcome;
        }

        public RuleOutcome IsCommentVoteAllowed(int commentID, int vote)
        {

            Context.CommentID = commentID;
            Context.VoteValue = vote;

            switch (vote)
            {
                case 1:
                    return EvaluateRuleSet(RuleScope.UpVoteComment);
                    break;
                case -1:
                    return EvaluateRuleSet(RuleScope.DownVoteComment);
                    break;

            }

            return RuleOutcome.Allowed;

        }
        public RuleOutcome IsSubmissionVoteAllowed(int submissionID, int vote)
        {

            Context.SubmissionID = submissionID;
            Context.VoteValue = vote;

            switch (vote)
            {
                case 1:
                    return EvaluateRuleSet(RuleScope.UpVoteSubmission);
                    break;
                case -1:
                    return EvaluateRuleSet(RuleScope.DownVoteSubmission);
                    break;

            }

            return RuleOutcome.Allowed;

        }
        public RuleOutcome IsPostCommentAllowed(int submissionID)
        {

            Context.SubmissionID = submissionID;
            return EvaluateRuleSet(RuleScope.PostComment);

        }



    }
}