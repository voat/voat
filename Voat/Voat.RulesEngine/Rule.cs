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
using System.Text.RegularExpressions;

namespace Voat.RulesEngine
{
    public abstract class Rule
    {
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
        public Rule(string name, string number, RuleScope scope, int order = 100)
        {
            if (String.IsNullOrEmpty(name) || name.Trim().Length == 0)
            {
                throw new RuleException("Rules must be provided with a rule name.");
            }

            if (String.IsNullOrEmpty(number))
            {
                throw new RuleException("Rules must be provided with a rule number.");
            }

            if (!Regex.IsMatch(number, @"^[1-9]{1}(?:\d{1})?((\.\d{1,2}){0,2})?$"))
            {
                throw new RuleException("Rule numbers must be in the form ##.##[.##]. Number can not start with a 0 (zero).");
            }

            _name = name.Trim();
            _scope = scope;
            _number = number.ToString();
            _order = order;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Number
        {
            get { return _number; }
        }

        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public RuleScope Scope
        {
            get { return _scope; }
        }

        protected RuleOutcome Allowed
        {
            get
            {
                return new RuleOutcome(RuleResult.Allowed, this.Name, this.Number, "Action Allowed");
            }
        }

        protected RuleOutcome CreateOutcome(RuleResult result, string message)
        {
            return new RuleOutcome(result, this.Name, this.Number, message);
        }

        protected RuleOutcome CreateOutcome(RuleResult result, string format, params object[] args)
        {
            return CreateOutcome(result, String.Format(format, args));
        }

        #region Object Overides

        //We want fast retrieval of objects so we override these methods to allow hashtable bucket sorting on scope
        public override int GetHashCode()
        {
            return (int)this.Scope;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Rule))
            {
                return false;
            }

            //I don't think we need this logic
            //Rule rule = (obj as Rule);
            //if (rule.Scope != this.Scope || rule.Name != this.Name) {
            //    return false;
            //}

            return ReferenceEquals(this, obj);
        }

        #endregion Object Overides
    }

    public abstract class Rule<T> : Rule where T : IRequestContext
    {
        public Rule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order)
        {
        }

        public RuleOutcome Evaluate(T context)
        {
            DemandContext(context);
            return EvaluateRule(context);
        }

        public bool TryEvaluate(T context, out RuleOutcome outcome)
        {
            outcome = null;
            try
            {
                outcome = EvaluateRule(context);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected abstract RuleOutcome EvaluateRule(T context);

        //ProtoType feature: Allows rules to describe the needed context and datatypes they require to execute correctly.
        public virtual IDictionary<string, Type> RequiredContext
        {
            get
            {
                return null;// new Dictionary<string, Type>() { { "SubmissionID", typeof(int) }, { "UserName", typeof(int) } };
            }
        }

        //Only run this code during DEBUG builds
        [Conditional("DEBUG")]
        protected void DemandContext(T context)
        {
            var requiredContext = RequiredContext;
            if (requiredContext != null)
            {
                foreach (var key in requiredContext.Keys)
                {
                    var type = requiredContext[key];
                    if (!context.Contains(key, type, true))
                    {
                        throw new RuleException(String.Format("Required context item '{0}' of type {1} does not exist in {2} Rule", key, (type == null ? "N/A" : type.Name), this.Name));
                    }
                }
            }
        }
    }
}
