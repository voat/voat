#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Voat.RulesEngine
{
    public abstract class RulesEngine<T> where T : IRequestContext
    {
        private bool _initialized = false;
        private bool _enabled = true;
        protected HashSet<Rule> _rules = new HashSet<Rule>();

        protected Func<Rule, Rule, bool> _validityAddCheck = new Func<Rule, Rule, bool>((existing, adding) =>
            existing.Number.Equals(adding.Number, StringComparison.OrdinalIgnoreCase)
            //            || existing.Name.Equals(adding.Name, StringComparison.OrdinalIgnoreCase)
            );

        public RulesEngine()
        {
            _rules = new HashSet<Rule>();
        }

        public void AddRule(Rule rule)
        {
            if (_rules.Any(x => _validityAddCheck(x, rule)))
            {
                throw new RuleException("Rules engine already contains a rule with the same name or number. '{0} ({1})'. Rules must be not share the same name or number.", rule.Name, rule.Number);
            }
            if (!_rules.Contains(rule))
            {
                _rules.Add(rule);
            }
        }

        public IEnumerable<Rule> Rules
        {
            get
            {
                return _rules;
            }
        }

        public bool Initialized
        {
            get
            {
                return _initialized;
            }

            set
            {
                _initialized = value;
            }
        }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = value;
            }
        }
        public void Initialize(RulesConfiguration config)
        {
            if (!Initialized)
            {
                lock (this)
                {
                    if (!Initialized)
                    {
                        Debug.Print("Starting: Initializing RulesEngine");
                        if (config != null && config.Enabled)
                        {
                            if (config.DiscoverRules)
                            {
                                if (!String.IsNullOrEmpty(config.DiscoverAssemblies))
                                {
                                    string[] assemblies = config.DiscoverAssemblies.Split(';');
                                    foreach (string assemblyName in assemblies)
                                    {
                                        try
                                        {
                                            var assembly = Assembly.Load(assemblyName);
                                            if (assembly != null)
                                            {
                                                LoadDiscoverableRules(assembly);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new RuleException(String.Format("Error loading aseembly or discoverable rules in assembly: {0}", assemblyName), ex);
                                        }
                                    }
                                }
                            }
                            //load rules specified in config file
                            if (config.Rules != null && config.Rules.Count > 0)
                            {
                                foreach (var ruleMeta in config.Rules)
                                {
                                    Type t = Type.GetType(ruleMeta.Type);
                                    if (t != null)
                                    {
                                        var ruleDescription = t.GetCustomAttribute<RuleDiscoveryAttribute>();
                                        var ruleInfo = RuleDiscoveryProvider.Map(new Tuple<Type, RuleDiscoveryAttribute>(t, ruleDescription));

                                        //force enabled from config
                                        ruleInfo.Enabled = ruleMeta.Enabled;

                                        if (ruleInfo.Enabled)
                                        {
                                            AddRule(ruleInfo.Rule);
                                        }
                                    }
                                    else
                                    {
                                        throw new RuleException(String.Format("Can not load rule type. Type: {0}", ruleMeta.Type));
                                    }
                                }
                            }
                        }
                        Debug.Print("Finished: Initializing RulesEngine");
                        Initialized = true;
                    }
                }
            }
        }

        protected virtual IEnumerable<Rule> GetRulesByScope(RuleScope scope, bool includeGlobalScope, Func<Rule, RuleScope, bool> scopeEvaluator)
        {
            if (scopeEvaluator == null)
            {
                return _rules.Where(x => (x.Scope == scope || (includeGlobalScope && x.Scope == RuleScope.Global))).OrderBy(x => x.Order).ToList();
            }
            else
            {
                return _rules.Where(x => scopeEvaluator(x, scope) || (includeGlobalScope && x.Scope == RuleScope.Global)).OrderBy(x => x.Order).ToList();
            }
        }

        public virtual RuleOutcome EvaluateRuleSet(T context, RuleScope[] ruleScopes, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            foreach (var scope in ruleScopes)
            {
                IEnumerable<Rule> rules = GetRulesByScope(scope, includeGlobalScope, scopeEvaluator);
                if (rules != null && rules.Any())
                {
                    foreach (var rule in rules)
                    {
                        Debug.Print(String.Format("Rule: {0}: {1} ({2} - {3})", rule.Name, rule.Number, scope.ToString(), typeof(Rule).Name));
                        RuleOutcome outcome = ((Rule<T>)rule).Evaluate(context);
                        if (!outcome.IsAllowed)
                        {
                            //rule pipeline failed :(
                            return outcome;
                        }
                    }
                }
            }
            return RuleOutcome.Allowed;
        }

        #region Dynamic Discovery of Rule objects to Load

        private void LoadDiscoverableRules(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            //by default use calling assembly to discover
            var rules = RuleDiscoveryProvider.DiscoverRules(new List<Assembly>() { assembly });

            if (rules != null && rules.Count() > 0)
            {
                var enabledRules = rules.Where(x => x.Enabled);
                foreach (var rule in enabledRules)
                {
                    AddRule(rule.Rule);
                }
            }
        }

        #endregion Dynamic Discovery of Rule objects to Load

        /// <summary>
        /// This method is a helper method to log error if Rules can't be constructed
        /// </summary>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        public static Rule ConstructRule(Type ruleType, bool suppressTypeLoadException = true)
        {
            Rule r = null;

            try
            {
                r = (Rule)Activator.CreateInstance(ruleType);
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("Rule '{0}' threw an error during construction. Ensure it has a default parameterless constructor and isn't abstract or static.", ruleType.Name);

                Trace.WriteLine(errorMessage);
                Debug.Print(errorMessage);

                if (!suppressTypeLoadException)
                {
                    throw new TypeLoadException(errorMessage, ex);
                }
            }
            return r;
        }
    }
}
