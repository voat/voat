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
    public enum RuleAction
    {
        Create = 1,
        Edit = 2,
        Delete = 4,
        View = 8,
        UpVote = 16,
        DownVote = 32
    }

    public enum RuleArea
    {
        Comment = 128,
        Submission = 256,
        Subverse = 512,
        Profile = 1024,
        Message = 2048,
        //Set = 2048
    }

    /// <summary>
    /// The scope at which this rule applies
    /// </summary>
    public enum RuleScope : int
    {
        /// <summary>
        /// Applies globally to every action.
        /// </summary>
        Global = 0,

        #region C-C-C-COMBOs
        /// <summary>
        /// Applies to editing of comment content
        /// </summary>
        EditComment = RuleAction.Edit | RuleArea.Comment,

        /// <summary>
        /// Applies to any posting of new content
        /// </summary>
        Post = RuleAction.Create,

        /// <summary>
        /// Applies to the posting of new comments
        /// </summary>
        PostComment = RuleAction.Create | RuleArea.Comment,

        /// <summary>
        /// Applies to the posting of new submissions
        /// </summary>
        PostSubmission = RuleAction.Create | RuleArea.Submission,

        /// <summary>
        /// Applies to the editing of existing submissions
        /// </summary>
        EditSubmission = RuleAction.Edit | RuleArea.Submission,

        /// <summary>
        /// Applies to the posting of new submissions
        /// </summary>
        PostMessage = RuleAction.Create | RuleArea.Message,

        /// <summary>
        /// Applies to the upvoting of comments
        /// </summary>
        UpVoteComment = RuleAction.UpVote | RuleArea.Comment,

        /// <summary>
        /// Applies to the downvoting of comments
        /// </summary>
        DownVoteComment = RuleAction.DownVote | RuleArea.Comment,

        /// <summary>
        /// Applies to the upvoting of submissions
        /// </summary>
        UpVoteSubmission = RuleAction.UpVote | RuleArea.Submission,

        /// <summary>
        /// Applies to the downvoting of submissions
        /// </summary>
        DownVoteSubmission = RuleAction.DownVote | RuleArea.Submission,

        /// <summary>
        /// Applies to any up vote operation
        /// </summary>
        UpVote = RuleAction.UpVote,

        /// <summary>
        /// Applies to any down vote operation
        /// </summary>
        DownVote = RuleAction.DownVote,

        /// <summary>
        /// Applies to any vote operation
        /// </summary>
        Vote = RuleAction.DownVote | RuleAction.UpVote,

        /// <summary>
        /// Applies to any comment vote operation
        /// </summary>
        VoteComment = Vote | RuleArea.Comment,

        /// <summary>
        /// Applies to any comment vote operation
        /// </summary>
        VoteSubmission = Vote | RuleArea.Submission,

        /// <summary>
        /// Applies to viewing of a subverse
        /// </summary>
        ViewSubverse = RuleAction.View | RuleArea.Subverse,

        /// <summary>
        /// Applies to creating a new subverse
        /// </summary>
        CreateSubverse = RuleAction.Create | RuleArea.Subverse

        #endregion C-C-C-COMBOs

    }
}
