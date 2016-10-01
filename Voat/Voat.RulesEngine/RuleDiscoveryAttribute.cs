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

namespace Voat.RulesEngine
{
    /// <summary>
    /// Describes a Rule
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RuleDiscoveryAttribute : Attribute
    {
        /// <summary>
        /// This attribute helps the UI display and show the rules that govern behavior on Voat.
        /// </summary>
        /// <remarks>This method will not enable this rule. Set Enabled = true to make rule active</remarks>
        /// <param name="enabled">Tells the discovery provider to load this rule into the rules engine. True for an active rule, False for an inactive rule</param>
        public RuleDiscoveryAttribute(bool enabled) : this(enabled, "", "")
        {
        }

        /// <summary>
        /// This attribute helps the UI display and show the rules that govern behavior on Voat.
        /// </summary>
        /// <remarks>This method will not enable this rule. Set Enabled = true to make rule active</remarks>
        /// <param name="description">The description of the rule. Use positive phrasing when describing the rule, i.e., 'Approves the action if user...' rather than 'Denies the action if user....'</param>
        /// <param name="psuedoLogic">The psuedo logic of the rule. Example: approved = (user.CCP > 20). Use positive phrasing when writing this logic.</param>
        public RuleDiscoveryAttribute(string description, string psuedoLogic) : this(true, description, psuedoLogic)
        {
        }

        /// <summary>
        /// This attribute helps the UI display and show the rules that govern behavior on Voat.
        /// </summary>
        /// <param name="enabled">Tells the discovery provider to load this rule into the rules engine. True for an active rule, False for an inactive rule</param>
        /// <param name="description">The description of the rule. Use positive phrasing when describing the rule, i.e., 'Approves the action if user...' rather than 'Denies the action if user....'</param>
        /// <param name="psuedoLogic">The psuedo logic of the rule. Example: approved = (user.CCP > 20). Use positive phrasing when writing this logic.</param>
        public RuleDiscoveryAttribute(bool enabled, string description, string psuedoLogic)
        {
            this.Enabled = enabled;
            this.Description = description;
            this.PsuedoLogic = psuedoLogic;
        }

        public string Description { get; set; }

        public string PsuedoLogic { get; set; }

        public bool Enabled { get; set; }
    }
}
