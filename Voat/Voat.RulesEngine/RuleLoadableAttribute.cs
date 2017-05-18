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
