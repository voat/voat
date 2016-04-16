using System;

namespace Voat.Domain.Models
{
    public enum CommentSort
    {
        New,
        Top
    }

    [Flags]
    //Don't change these values
    public enum ContentType
    {
        Submission = 1,
        Comment = 2
    }

    public enum SubscriptionAction
    {
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

    public enum Origin
    {
        /// <summary>
        /// From the web UI
        /// </summary>
        UI = 0,

        /// <summary>
        /// From the API endpoints
        /// </summary>
        API = 1,

        /// <summary>
        /// From client-side JS
        /// </summary>
        AJAX = 2
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

        /// <summary>
        /// Orders results by intensity of up/down votes
        /// </summary>
        Chaos, //order by degree of up vs down compared to sum of votes
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

    public enum SubmissionType
    {
        Text = 1,
        Link = 2
    }

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
}
