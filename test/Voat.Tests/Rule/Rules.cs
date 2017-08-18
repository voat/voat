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

using Voat.Common;
using Voat.RulesEngine;

namespace Voat.Tests.Rules
{
    [RuleDiscovery(false)]
    public class TestDeniedRule : TestRule
    {
        public TestDeniedRule() : base("Test Denial", "1.1", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            return CreateOutcome(RuleResult.Denied, "Test denied");
        }
    }

    [RuleDiscovery(true)]
    public class TestPassRule : TestRule
    {
        public TestPassRule() : base("Test Pass", "1.0", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            return Allowed;
        }
    }

    public class TestRule : Rule<RequestContext>
    {
        public TestRule(string name, string number, RuleScope scope) : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            //context.PropertyBag
            return Allowed;
        }
    }
}
