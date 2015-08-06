using System;

namespace Voat.Models
{

    #region Enumerations 

    //It is CRITICAL these roles are numbered correctly as the value each one contains is used to rank 
    //the role in permission based tasks. Where this is critical is role assignment. An Admin can not 
    //change the permissions of a GlobalAdmin as a GlobalAdmin outranks them (Higer Value). A DelegateAdmin can not 
    //modify permissions of an Admin or GlobalAdmin. 
    public enum UserRole : int
    {
        GlobalAdmin = 2147483647,
        Admin = 2147483646,
        DelegateAdmin = 10000
    }

    public enum Origin
    {
        UI = 0,
        API = 1,
        UIJS = 2
    }
    [Flags]
    //Don't change these values
    public enum ContentType
    {
        Submission = 1,
        Comment = 2
    }
    public enum SubmissionType
    {
        Self = 1,
        Link = 2
    }
    /// <summary>
    /// The result of a vote operation
    /// </summary>
    public enum ProcessResult
    {
        /// <summary>
        /// Vote operation was not processed.
        /// </summary>
        NotProcessed = 0,
        /// <summary>
        /// Vote operation was successfully recorded
        /// </summary>
        Success = 1,
        /// <summary>
        /// Vote operation was ignored by the system. Reasons usually include a duplicate vote or a vote on a non-voteable item.
        /// </summary>
        Ignored = 2,
        /// <summary>
        /// Vote operation was denied by the system. Typically this response is returned when user doesn't have the neccessary requirements to vote on item.
        /// </summary>
        Denied = 3
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

    /// <summary>
    /// The type of messages to get for a user
    /// </summary>
    [Flags]
    public enum MessageType
    {

        /// <summary>
        /// Private Messages
        /// </summary>
        Inbox = 1,

        /// <summary>
        /// Sent Private Messages
        /// </summary>
        Sent = 2,

        /// <summary>
        /// Comment Reply Messages
        /// </summary>
        Comment = 4,

        /// <summary>
        /// Submission Reply Messages
        /// </summary>
        Submission = 8,

        /// <summary>
        /// User Mention Messages
        /// </summary>
        Mention = 16,

        /// <summary>
        /// All Messages
        /// </summary>
        All = Inbox | Sent | Comment | Submission | Mention
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
        /// Orders results by relative ranking
        /// </summary>
        Hot, //order by rank

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
    /// Specifies the type of Subscription 
    /// </summary>
    public enum SubscriptionType
    {

        /// <summary>
        /// Represents a subverse subscription
        /// </summary>
        Subverse = 1,
        /// <summary>
        /// Represents a set subscription
        /// </summary>
        Set = 2
    }

    #endregion

}
