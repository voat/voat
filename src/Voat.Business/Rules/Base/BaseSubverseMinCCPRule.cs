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
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public abstract class BaseSubverseMinimumCCPRule : VoatRule
    {
        public BaseSubverseMinimumCCPRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        public override IDictionary<string, Type> RequiredContext
        {
            get
            {
                return new Dictionary<string, Type>() { { "Subverse", typeof(Subverse) } };
            }
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var subverse = context.Subverse;

            int? subMinCCP = subverse.MinCCPForDownvote;

            if (subMinCCP.HasValue && subMinCCP.Value > 0)
            {
                var q = new QueryUserContributionPoints(Domain.Models.ContentType.Comment, subverse.Name).SetUserContext(context.User);
                var score = q.Execute();

                if (score.Sum < subMinCCP.Value)
                {
                    return CreateOutcome(RuleResult.Denied, String.Format("Subverse '{0}' requires {1}CCP to downvote", subverse.Name, subMinCCP.Value.ToString()));
                }
            }

            return base.EvaluateRule(context);
        }
    }
}
