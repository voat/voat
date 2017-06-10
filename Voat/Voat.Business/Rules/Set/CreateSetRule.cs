#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
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
            var systemSets = Enum.GetNames(typeof(SetType)).Concat(Enum.GetNames(typeof(SortAlgorithm)));
            if (systemSets.Any(x => x.IsEqual(set.Name)))
            {
                return base.CreateOutcome(RuleResult.Denied, "Set name is in a restricted list - please choose a different name");
            }
           
            ////Age
            //var registrationDate = context.UserData.Information.RegistrationDate;
            //int accountAgeInDays = Repository.CurrentDate.Subtract(registrationDate).Days;
            //if (accountAgeInDays < VoatSettings.Instance.MinimumAccountAgeInDaysForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires an account age of {VoatSettings.Instance.MinimumAccountAgeInDaysForSubverseCreation} days");
            //}
            ////CCP
            //if (context.UserData.Information.CommentPoints.Sum < VoatSettings.Instance.MinimumCommentPointsForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {VoatSettings.Instance.MinimumCommentPointsForSubverseCreation} comment (CCP) points");
            //}
            ////SCP
            //if (context.UserData.Information.SubmissionPoints.Sum < VoatSettings.Instance.MinimumSubmissionPointsForSubverseCreation)
            //{
            //    return base.CreateOutcome(RuleResult.Denied, $"Sorry, subverse creation requires a minimum of {VoatSettings.Instance.MinimumSubmissionPointsForSubverseCreation} submission (SCP) points");
            //}

            //rules checkd in base class
            return base.EvaluateRule(context);
        }
    }
}
