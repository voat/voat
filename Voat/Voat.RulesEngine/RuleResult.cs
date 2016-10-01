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

namespace Voat.RulesEngine
{
    /// <summary>
    /// The result of a Operation
    /// </summary>
    public enum RuleResult
    {
        /// <summary>
        /// Operation was not processed.
        /// </summary>
        Unevaluated = 0,

        /// <summary>
        /// Vote operation was successfully recorded
        /// </summary>
        Allowed = 1,

        ///// <summary>
        ///// Operation was ignored by the system. Reasons usually include a duplicate vote or a vote on a non-voteable item.
        ///// </summary>
        //Ignored = 2,

        /// <summary>
        /// Operation was denied by the system. Typically this response is returned when user doesn't have the neccessary requirements to complete operation.
        /// </summary>
        Denied = 3
    }
}
