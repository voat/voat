using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.RulesEngine
{

    /// <summary>
    /// The result of a Operation
    /// </summary>
    public enum RuleResult {
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