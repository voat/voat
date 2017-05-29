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
using Voat.RulesEngine;

namespace Voat.Rules
{
    public class VoatRulesEngine : RulesEngine<VoatRuleContext>
    {
        protected static VoatRulesEngine _engine;

        protected VoatRulesEngine(/*IRequestContextHandler<VoatRequestContext> handler*/) /*: base(handler)*/
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
                            _engine = new VoatRulesEngine(/*new RequestHttpContextHandler()*/);
                            _engine.Initialize(RuleConfigurationSettings.Instance);
                            _engine.Enabled = RuleConfigurationSettings.Instance.Enabled;
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

        public override RuleOutcome EvaluateRuleSet(VoatRuleContext context, RuleScope[] ruleScopes, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            var outcome = RuleOutcome.Allowed;

            if (this.Enabled)
            {
                outcome = base.EvaluateRuleSet(context, ruleScopes, includeGlobalScope, scopeEvaluator);
            }

            return outcome;
        }

        public RuleOutcome EvaluateRuleSet(VoatRuleContext context, RuleScope scope, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            return EvaluateRuleSet(context, new RuleScope[] { scope }, includeGlobalScope, scopeEvaluator);
        }

        public RuleOutcome EvaluateRuleSet(VoatRuleContext context, params RuleScope[] scopes)
        {
            return EvaluateRuleSet(context, scopes, true, null);
        }
    }
}
