using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.RulesEngine
{


    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class RuleLoadableAttribute : Attribute {

        public RuleLoadableAttribute() {
            Enabled = true;
        }

        public RuleLoadableAttribute(bool enabled) {
            Enabled = enabled;
        }
        /// <summary>
        /// This attribute helps the UI display and show the rules that govern behavior on Voat.
        /// </summary>
        /// <param name="description">The description of the rule. Use positive phrasing when describing the rule, i.e., 'Approves the action if user...' rather than 'Denies the action if user....'</param>
        /// <param name="psuedoLogic">The psuedo logic of the rule. Example: approved = (user.CCP > 20). Use positive phrasing when writing this logic.</param>
        public RuleLoadableAttribute(bool enabled, string description, string psuedoLogic) {
            this.Description = description;
            this.PsuedoLogic = psuedoLogic;
            this.Enabled = enabled;
        }

        public string Description {get;set;}
        public string PsuedoLogic {get; set;}
        public bool Enabled {get; set;}

}


}