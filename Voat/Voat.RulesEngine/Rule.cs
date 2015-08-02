using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Voat.RulesEngine
{

    public abstract class Rule {

        private string _name = null;
        private string _number = null;
        private RuleScope _scope = RuleScope.Global;
        private int _order = 100;

        /// <summary>
        /// The abstract rule constructor
        /// </summary>
        /// <param name="name">The name of this rule. Must be globally unique among all rules.</param>
        /// <param name="number">The number of this rule in format [D]D.DD[.DD]. Must be globally unique among all rules.</param>
        /// <param name="scope">The scope that this rule applies. Direct matches are searched for when applying rule logic</param>
        /// <param name="order">The order in which a rule should run relative to it's scope. Rules are ordered ascending by this value.</param>
        public Rule(string name, string number, RuleScope scope, int order = 100) {

            if (String.IsNullOrEmpty(name) || name.Trim().Length == 0) {
                throw new RuleException("Rules must be provided with a rule name.");
            }
            if (String.IsNullOrEmpty(number)) {
                throw new RuleException("Rules must be provided with a rule number.");
            }
            if (!Regex.IsMatch(number, @"^[1-9]{1}(?:\d{1})?((\.\d{1,2}){0,2})?$")) {
                throw new RuleException("Rule numbers must be in the form ##.##[.##]. Number can not start with a 0 (zero).");
            }
            _name = name.Trim();
            _scope = scope;
            _number = number;
            _order = order;
        }

        public abstract RuleOutcome Evaluate();

        public virtual bool TryEvaluate(out RuleOutcome outcome){

            outcome = null;

            try {
                outcome = Evaluate();
                return true;
            } catch (Exception ex){
                return false;
            }

        }
        //ProtoType feature: Allows rules to describe the needed context and datatypes they require to execute correctly.
        public virtual IDictionary<string, Type> RequiredContext { 
            get {
                return null;
            } 
        }

        public string Name {
            get { return _name; }
        }
        public string Number {
            get { return _number; }

        }

        public int Order {
            get { return _order; }
        }

        public RuleScope Scope {
            get { return _scope; }
        }

        protected RuleOutcome Allowed {
            get {
                return new RuleOutcome(RuleResult.Allowed, this.Name, this.Number, "Action Allowed");
            }
        }
        protected RuleOutcome CreateOutcome(RuleResult result, string message) {
            return new RuleOutcome(result, this.Name, this.Number, message);
        }

        protected RuleOutcome CreateOutcome(RuleResult result, string format, params object[] args) {
            return CreateOutcome(result, String.Format(format, args));
        }

        #region Object Overides
        //We want fast retrieval of objects so we override these methods to allow hashtable bucket sorting on scope
        public override int GetHashCode() {
            return (int)this.Scope;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (!(obj is Rule)) {
                return false;
            }

            //I don't think we need this logic 
            //Rule rule = (obj as Rule);
            //if (rule.Scope != this.Scope || rule.Name != this.Name) {
            //    return false;
            //}

            return ReferenceEquals(this, obj);

        }
        #endregion
    }
}
