using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Data;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    [RuleDiscovery("Approves a subverse creation if conditions are valid.", "approved = (command.IsValid())")]
    public class CreateSubverseNameRule : VoatRule
    {
        public CreateSubverseNameRule()
            : base("Subverse Name", "8.0", RuleScope.CreateSubverse)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            string name = context.PropertyBag.SubverseName;

            if (String.IsNullOrWhiteSpace(name))
            {
                return base.CreateOutcome(RuleResult.Denied, "Subverses must have a valid name");
            }

            if (!Regex.IsMatch(name, String.Concat("^", CONSTANTS.SUBVERSE_REGEX, "$")))
            {
                return base.CreateOutcome(RuleResult.Denied, "Subverse name does not conform to naming requirements");
            }

            using (var repo = new Repository())
            {
                if (repo.SubverseExists(name))
                {
                    return base.CreateOutcome(RuleResult.Denied, "Sorry, The subverse you are trying to create already exists, but you can try to claim it by submitting a takeover request to /v/subverserequest");
                }
                var subs = repo.GetSubversesUserModerates(context.UserName);
                if (subs.Count(x => x.Power <= 2) >= Settings.MaximumOwnedSubs)
                {
                    return base.CreateOutcome(RuleResult.Denied, "Sorry, you can not moderate more than " + Settings.MaximumOwnedSubs + " subverses.");
                }
            }

            //Age
            var registrationDate = context.UserData.Information.RegistrationDate;
            int accountAgeInDays = Repository.CurrentDate.Subtract(registrationDate).Days;
            if (accountAgeInDays < Settings.MinimumAccountAgeInDaysForSubverseCreation)
            {
                return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires an account age of {Settings.MinimumAccountAgeInDaysForSubverseCreation} days");
            }
            //CCP
            if (context.UserData.Information.CommentPoints.Sum < Settings.MinimumCommentPointsForSubverseCreation)
            {
                return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {Settings.MinimumCommentPointsForSubverseCreation} comment (CCP) points");
            }
            //SCP
            if (context.UserData.Information.SubmissionPoints.Sum < Settings.MinimumSubmissionPointsForSubverseCreation)
            {
                return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {Settings.MinimumSubmissionPointsForSubverseCreation} submission (SCP) points");
            }

            //rules checkd in base class
            return base.EvaluateRule(context);
        }
    }
}
