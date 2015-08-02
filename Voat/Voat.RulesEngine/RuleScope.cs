using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.RulesEngine
{

    public enum RuleAction { 
        Create,
        Edit,
        Delete,
        View
    }

    public enum RuleArea { 
        Comment,
        Submission,
        Subverse,
        Set
    }

    /// <summary>
    /// The scope at which this rule applies
    /// </summary>
    public enum RuleScope : int {

        /// <summary>
        /// Applies globally to every action. 
        /// </summary>
        Global = -1,

        /// <summary>
        /// Applies to any downvote action
        /// </summary>
        DownVote = 1,

        /// <summary>
        /// Applies to any upvote action
        /// </summary>
        UpVote = 2,

        /// <summary>
        /// Applies to any submission action
        /// </summary>
        Submission = 4,

        /// <summary>
        /// Applies to any comment action
        /// </summary>
        Comment = 8,

        /// <summary>
        /// Applies to any subverse action
        /// </summary>
        Subverse = 16,


        Message = 32,
        /// <summary>
        /// Applies to any post action (posting of both comments and submissions)
        /// </summary>
        Post = 16,
        
        //Create = Post,

        Delete = 32,
        
        Edit = 64,

        View = 128,

        ///// <summary>
        ///// MAYBE? Applies to any view action (posting of both comments and submissions)
        ///// </summary>
        //View = 32,

        #region CccccCOMBOs

        /// <summary>
        /// Applies to the posting of new comments
        /// </summary>
        PostComment = Post | Comment,

        /// <summary>
        /// Applies to the posting of new submissions
        /// </summary>
        PostSubmission = Post | Submission,

        /// <summary>
        /// Applies to the upvoting of comments
        /// </summary>
        UpVoteComment = UpVote | Comment,

        /// <summary>
        /// Applies to the downvoting of comments
        /// </summary>
        DownVoteComment = DownVote | Comment,

        /// <summary>
        /// Applies to the upvoting of submissions
        /// </summary>
        UpVoteSubmission = UpVote | Submission,

        /// <summary>
        /// Applies to the downvoting of submissions
        /// </summary>
        DownVoteSubmission = DownVote | Submission,

        /// <summary>
        /// Applies to any vote operation
        /// </summary>
        Vote = DownVote | UpVote

        //,
        //ViewComment = View | Comment,
        //ViewSubmission = View | Submission

        #endregion 

    }

}