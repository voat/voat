using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Voat.RulesEngine
{

    

    public abstract class RulesEngine<T> where T : RuleContext {

        //we want fast retrieval of rules based on scope
        protected HashSet<Rule> _rules = new HashSet<Rule>();
        protected IRuleContextHandler<T> _contextHandler;
        protected Func<Rule, Rule, bool> _validityAddCheck = new Func<Rule, Rule, bool>((existing, adding) => 
            existing.Number.Equals(adding.Number, StringComparison.OrdinalIgnoreCase) 
//            || existing.Name.Equals(adding.Name, StringComparison.OrdinalIgnoreCase)
            );
        public RulesEngine(IRuleContextHandler<T> contextHandler) {

            if (contextHandler == null) {
                throw new RuleException("IRuleContextHandler object can not be null.");
            }

            _rules = new HashSet<Rule>();
            _contextHandler = contextHandler;
        }

        public void AddRule(Rule rule) {

            if (_rules.Any(x => _validityAddCheck(x, rule))) {
                throw new RuleException("Rules engine already contains a rule with the same name or number. '{0} ({1})'. Rules must be not share the same name or number.", rule.Name, rule.Number);
            }
            _rules.Add(rule);
        
        }

        public IEnumerable<Rule> Rules {
            get {
                return _rules;
            }
        }
        
        public virtual T Context {
            get {
                return _contextHandler.Context;
            }
        }

        protected virtual List<Rule> GetRulesByScope(RuleScope scope, bool includeGlobalScope, Func<Rule, RuleScope, bool> scopeEvaluator) {
            
            if (scopeEvaluator == null) {
                return _rules.Where(x => (x.Scope == scope || (includeGlobalScope && x.Scope == RuleScope.Global))).OrderBy(x => x.Order).ToList();
            } else {
                return _rules.Where(x => scopeEvaluator(x, scope) || (includeGlobalScope && x.Scope == RuleScope.Global)).OrderBy(x => x.Order).ToList();
            }
        }

        public virtual RuleOutcome EvaluateRuleSet(RuleScope scope, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null) {

            List<Rule> rules = GetRulesByScope(scope, includeGlobalScope, scopeEvaluator);

            if (rules != null && rules.Count > 0) {
                foreach (var rule in rules) {
                    RuleOutcome outcome = rule.Evaluate();
                    if (!outcome.IsAllowed) {
                        //rule pipeline failed :(
                        return outcome;
                    }
                }
            }

            return RuleOutcome.Allowed;        
        }

        #region Dynamic Discovery of Rule objects to Load

        public void LoadDiscoverableRules(Assembly assembly) {

            if (assembly == null) {
                return;
            }

            //by default use calling assembly to discover
            var rulePairs = RuleDiscoveryProvider.DiscoverRules(new List<Assembly>(){assembly});

            if (rulePairs != null && rulePairs.Count() > 0) {
                foreach (var rulePair in rulePairs) {
                    if (rulePair.Item2.Enabled) {
                        Rule r = ConstructRule(rulePair.Item1, false); //we want exceptions thrown during load because base Rule constructor validates input subclass info. 
                        if (r != null) {
                            AddRule(r);
                        }
                    }
                }
            }

        }

        #endregion
        /// <summary>
        /// This method is a helper method to log error if Rules can't be constructed
        /// </summary>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        public static Rule ConstructRule(Type ruleType, bool suppressTypeLoadException = true) {
            Rule r = null;

            try {
                r = (Rule)Activator.CreateInstance(ruleType);
            } catch (Exception ex) {

                string errorMessage = String.Format("Rule '{0}' threw an error during construction. Ensure it has a default parameterless constructor and isn't abstract or static.", ruleType.Name);

                Trace.WriteLine(errorMessage);
                Debug.Print(errorMessage);
                
                if (!suppressTypeLoadException){
                    throw new TypeLoadException(errorMessage, ex);
                }
            }

            return r;
        }

    }


    

   
   

   


   

}