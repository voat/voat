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

namespace Voat.Domain.Models
{
    public enum VoteValue
    {
        None = 0,
        Up = 1,
        Down = -1
    }

    //public enum CommentSort
    //{
    //    New,
    //    Top
    //}

    [Flags]
    [DatabaseMappedValue]
    //Don't change these values
    public enum ContentType
    {
        Submission = 1,
        Comment = 2,
        Vote = 3
    }

    public enum SubscriptionAction
    {
        Toggle = 0,
        Subscribe = 1,
        Unsubscribe = 2
    }

    /// <summary>
    /// Specifies the type of Domain object
    /// </summary>
    public enum DomainType
    {
        /// <summary>
        /// Represents a subverse domain type
        /// </summary>
        Subverse = 1,

        /// <summary>
        /// Represents a set domain type
        /// </summary>
        Set = 2,

        /// <summary>
        /// Represents a user domain type
        /// </summary>
        User = 3
    }

    /// <summary>
    /// The type of messages to retrieve
    /// </summary>
    public enum MessageState
    {
        Unread = 1,
        Read = 2,
        All = Unread | Read,
    }
    //DO NOT CHANGE THESE VALUES - ALIGNS WITH DB
    [DatabaseMappedValue]
    public enum IdentityType
    {
        User = 1,
        Subverse = 2
    }
    //DO NOT CHANGE THESE VALUES - ALIGNS WITH DB
    [DatabaseMappedValue]
    public enum MessageType
    {
        Private = 1,
        Sent = 2,
        SubmissionMention = 3,
        CommentMention = 4,
        SubmissionReply = 5,
        CommentReply = 6,
    }

    /// <summary>
    /// The type of messages to get for a user from API
    /// </summary>
    [Flags]
    public enum MessageTypeFlag
    {
        /// <summary>
        /// Private Messages
        /// </summary>
        Private = 1,

        /// <summary>
        /// Sent Private Messages
        /// </summary>
        Sent = 2,

        /// <summary>
        /// Comment Reply Messages
        /// </summary>
        CommentReply = 4,

        ///// <summary>
        ///// Submission Reply Messages
        ///// </summary>
        SubmissionReply = 8,

        /// <summary>
        /// User Mention Messages
        /// </summary>
        CommentMention = 16,

        /// <summary>
        /// User Mention Messages
        /// </summary>
        SubmissionMention = 32,

        /// <summary>
        /// All Messages
        /// </summary>
        All = Private | Sent | CommentReply | CommentMention | SubmissionReply | SubmissionMention
    }

    [DatabaseMappedValue()]
    public enum CommentSortAlgorithm
    {
        /// <summary>
        /// Orders results by creation date
        /// </summary>
        New = 1, //order by date

        /// <summary>
        /// Orders results by creation date ascending
        /// </summary>
        Old, //order by date

        /// <summary>
        /// Orders results by sum of vote count
        /// </summary>
        Top, //order by total upvotes

        /// <summary>
        /// Orders results by sum of vote count reversed
        /// </summary>
        Bottom, //order by most downvotes

        /// <summary>
        /// Orders results by intensity of up/down votes
        /// </summary>
        Intensity,

        ///// <summary>
        ///// Orders results in a traditional forum style by date (uses new sort currently)
        ///// </summary>
        //Forum = New

    }

    /// <summary>
    /// Specifies the sort algorithm to apply to result set.
    /// </summary>
    public enum SortAlgorithm
    {
        /// <summary>
        /// Orders results by creation date
        /// </summary>
        New, //order by date

        /// <summary>
        /// Orders results by upvote count
        /// </summary>
        Top, //order by total upvotes

        /// <summary>
        /// Orders results by absolute ranking
        /// </summary>
        Rank, //order by rank

        /// <summary>
        /// Orders results by absolute ranking
        /// </summary>
        Hot = Rank, //order by rank

        /// <summary>
        /// Orders results by relative ranking (per subverse)
        /// </summary>
        RelativeRank, //order by rel rank

        /// <summary>
        /// Orders results by relative ranking (per subverse)
        /// </summary>
        Relative = RelativeRank, //order by rel rank

        /// <summary>
        /// Orders results by last comment date
        /// </summary>
        Active, //order by last comment time

        /// <summary>
        /// Orders results by view count.
        /// </summary>
        Viewed, //order by most views

        /// <summary>
        /// Orders results by comment count
        /// </summary>
        Discussed, //order by most comments

        /// <summary>
        /// Orders results by downvote count
        /// </summary>
        Bottom, //order by most downvotes

        /// <summary>
        /// Orders results by intensity of up/down votes
        /// </summary>
        //Chaos, //order by degree of up vs down compared to sum of votes

        /// <summary>
        /// Orders results by intensity of up/down votes
        /// </summary>
        Intensity
    }

    /// <summary>
    /// Specifies the direction result sets should be sorted.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Default sort order for result set
        /// </summary>
        Default,

        /// <summary>
        /// Reversed sort order for result set
        /// </summary>
        Reverse
    }

    /// <summary>
    /// Specifies the time span window to use to filter results in set.
    /// </summary>
    public enum SortSpan
    {
        /// <summary>
        /// Default value
        /// </summary>
        All = 0,

        /// <summary>
        /// Limits search span to 1 hour
        /// </summary>
        Hour,

        /// <summary>
        /// Limits search span to 1 day
        /// </summary>
        Day,

        /// <summary>
        /// Limits search span to 1 week
        /// </summary>
        Week,

        /// <summary>
        /// Limits search span to ~30 days
        /// </summary>
        Month,

        /// <summary>
        /// Limits search span to ~90 days
        /// </summary>
        Quarter,

        /// <summary>
        /// Limits search span to 1 year
        /// </summary>
        Year
    }
    //DO NOT CHANGE THESE VALUES - ALIGNS WITH DB
    [DatabaseMappedValue]
    public enum SubmissionType
    {
        Text = 1,
        Link = 2,
        //New types
        LinkText = 3, //Both a URL and a Body are provided
        Status = 4, //Only message content is provided - an update/selfsub post
        Vote = 5 //Discussion Thread for a Vote
    }

    //DO NOT CHANGE THESE VALUES - ALIGNS WITH DB
    [DatabaseMappedValue]
    public enum SubverseType
    {
        Subverse = 1,
        User = 2,
        Sponsored = 3
    }
    
    //It is CRITICAL these roles are numbered correctly as the value each one contains is used to rank
    //the role in permission based tasks. Where this is critical is role assignment. An Admin can not
    //change the permissions of a GlobalAdmin as a GlobalAdmin outranks them (Higer Value). A DelegateAdmin can not
    //modify permissions of an Admin or GlobalAdmin.
    public enum UserRole : int
    {
        GlobalAdmin = 2147483647,
        Admin = 2147483646,
        DelegateAdmin = 10000,
        GlobalBans = 9500,
        GlobalJanitor = 9001,
    }

    [DatabaseMappedValue]
    public enum ModeratorLevel
    {
        Owner = 1,
        Moderator = 2,
        Janitor = 3,
        Designer = 4,
        Submitter = 99
    }

    public enum ModeratorAction
    {
        ReadMail,
        SendMail,
        DeleteMail,
        DeletePosts,
        DeleteComments,
        Banning,
        AssignFlair,
        ModifyFlair,
        ModifyCSS,
        ModifySettings,
        InviteMods,
        RemoveMods,
        AssignStickies,
        DistinguishContent,
        AccessReports,
        MarkReports
    }

    public enum ReviewStatus
    {
        Any,
        Reviewed,
        Unreviewed
    }

    [DatabaseMappedValue()]
    public enum SetType
    {
        Front = 1,
        Blocked = 2,
        Following = 3,
        Normal = 4,
    }

    [Flags]
    public enum RobotIndexing
    {
        NoIndex = 1,
        NoFollow = 2,
        None = NoIndex | NoFollow,
        Follow = 4,
        Index = 8,
        All = Follow | Index
    }
    [Flags]
    public enum VoteTypeAllowed
    {
        None = 1,
        Up = 2,
        Down = 4,
        Any = Up | Down,
    }

    public enum BanType
    {
        User = 1,
        Domain = 2
    }
}
