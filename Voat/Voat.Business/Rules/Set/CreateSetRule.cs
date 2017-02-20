using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    [RuleDiscovery("Approves a set creation if conditions are valid.", "approved = (command.IsValid())")]
    public class CreateSetRule : VoatRule
    {
        public CreateSetRule()
            : base("Set Create", "8.1", RuleScope.CreateSet)
        {
            RequiredContext.Add("Set", typeof(Domain.Models.Set));
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            Domain.Models.Set set = context.PropertyBag.Set;

            if (String.IsNullOrWhiteSpace(set.Name))
            {
                return base.CreateOutcome(RuleResult.Denied, "Sets must have a valid name");
            }

            if (!Regex.IsMatch(set.Name, String.Concat("^", CONSTANTS.SUBVERSE_REGEX, "$")))
            {
                return base.CreateOutcome(RuleResult.Denied, "Set name does not conform to naming requirements");
            }
            //Ensure Name is not used for system sets 
            var systemSets = Enum.GetNames(typeof(SetType));
            if (systemSets.Any(x => x.IsEqual(set.Name)))
            {
                return base.CreateOutcome(RuleResult.Denied, "Set name is in restricted list");
            }

            ////Age
            //var registrationDate = context.UserData.Information.RegistrationDate;
            //int accountAgeInDays = Repository.CurrentDate.Subtract(registrationDate).Days;
            //if (accountAgeInDays < Settings.MinimumAccountAgeInDaysForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires an account age of {Settings.MinimumAccountAgeInDaysForSubverseCreation} days");
            //}
            ////CCP
            //if (context.UserData.Information.CommentPoints.Sum < Settings.MinimumCommentPointsForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {Settings.MinimumCommentPointsForSubverseCreation} comment (CCP) points");
            //}
            ////SCP
            //if (context.UserData.Information.SubmissionPoints.Sum < Settings.MinimumSubmissionPointsForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {Settings.MinimumSubmissionPointsForSubverseCreation} submission (SCP) points");
            //}

            //rules checkd in base class
            return base.EvaluateRule(context);
        }
    }
}
