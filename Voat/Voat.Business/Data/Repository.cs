using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq.Expressions;
using Voat.Domain.Query;
using Voat.Common;
using Voat.Data.Models;
using Voat.Models;
using Voat.Domain.Models;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Utilities;
using Voat.Utilities.Components;
using Voat.Domain.Command;
using System.Text.RegularExpressions;

namespace Voat.Data
{

    //READ ME: (You won't, you never do.) 
    //Nothing in this class is cached, it is all direct pulls. 
    //Nothing in this class is async (we might change this, but lets do all or nothing until then). 
    public class Repository : IDisposable
    {
        private static LockStore _lockStore = new LockStore();
        private voatEntities _db;

        public Repository() : this(new voatEntities())
        {
            /*no-op*/
        }

        public Repository(Models.voatEntities dbContext)
        {
            _db = dbContext;
            //Prevent EF from creating dynamic proxies, those mother fathers. This killed 
            //us during The Fattening, so we throw now -> (╯°□°)╯︵ ┻━┻ 
            _db.Configuration.ProxyCreationEnabled = false;
        }

        #region Vote

        public VoteResponse VoteComment(int commentID, int vote, string addressHash, bool revokeOnRevote = true)
        {
            DemandAuthentication();

            //make sure we don't have bad int values for vote
            if (Math.Abs(vote) > 1)
            {
                throw new ArgumentOutOfRangeException("vote", "Valid values for vote are only: -1, 0, 1");
            }

            string userName = User.Identity.Name;
            var ruleContext = new VoatRuleContext();
            ruleContext.PropertyBag.AddressHash = addressHash;
            RuleOutcome outcome = null;

            string REVOKE_MSG = "Vote has been revoked";
            Data.Models.Submission submission = null;

            var synclock_comment = _lockStore.GetLockObject(String.Format("comment:{0}", commentID));
            lock (synclock_comment)
            {
                var comment = _db.Comments.FirstOrDefault(x => x.ID == commentID);

                if (comment != null)
                {
                    //set properties for rules engine
                    ruleContext.CommentID = commentID;
                    ruleContext.SubmissionID = comment.SubmissionID;
                    
                    //execute rules engine 
                    switch (vote)
                    {
                        case 1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteComment, RuleScope.UpVote, RuleScope.UpVoteComment);
                            if (outcome.IsDenied)
                            {
                                return VoteResponse.Create(outcome);
                            }
                            break;
                        case -1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteComment, RuleScope.DownVote, RuleScope.DownVoteComment);
                            if (outcome.IsDenied)
                            {
                                return VoteResponse.Create(outcome);
                            }
                            break;
                    }

                    submission = _db.Submissions.First(x => x.ID == comment.SubmissionID);

                    //ignore votes if comment is users
                    if (String.Equals(comment.UserName, userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    var existingVote = 0;

                    var existingVoteTracker = _db.CommentVoteTrackers.FirstOrDefault(x => x.CommentID == commentID && x.UserName == userName);
                    if (existingVoteTracker == null)
                    {
                        //invoke comment address check
                    }
                    else if (existingVoteTracker != null && existingVoteTracker.VoteStatus.HasValue)
                    {
                        existingVote = existingVoteTracker.VoteStatus.Value;
                    }

                    // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                    if (existingVote == vote && !revokeOnRevote)
                    {
                        return VoteResponse.Ignored(existingVote, "User has already voted this way.");
                    }

                    VoteResponse response = new VoteResponse(Status.NotProcessed, 0, "Vote not processed.");
                    switch (existingVote)
                    {
                        case 0: //Never voted or No vote

                            switch (vote)
                            {
                                case 0:
                                    response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");
                                    break;
                                case 1:
                                case -1:

                                    if (vote == 1)
                                    {
                                        comment.UpCount++;
                                    }
                                    else
                                    {
                                        comment.DownCount++;
                                    }

                                    var newVotingTracker = new CommentVoteTracker
                                    {
                                        CommentID = commentID,
                                        UserName = userName,
                                        VoteStatus = vote,
                                        IPAddress = addressHash,
                                        CreationDate = Repository.CurrentDate
                                    };

                                    _db.CommentVoteTrackers.Add(newVotingTracker);
                                    _db.SaveChanges();

                                    //SendVoteNotification(comment.Name, "upvote");
                                    response = VoteResponse.Success(vote);
                                    response.Difference = vote;
                                    response.Response = new Score() { DownCount = (int)comment.DownCount, UpCount = (int)comment.UpCount };
                                    break;
                            }
                            break;
                        case 1: //Previous Upvote

                            switch (vote)
                            {
                                case 0: //revoke
                                case 1: //revote which means revoke if we are here

                                    if (existingVoteTracker != null)
                                    {

                                        comment.UpCount--;

                                        _db.CommentVoteTrackers.Remove(existingVoteTracker);

                                        _db.SaveChanges();

                                        response = VoteResponse.Success(0, REVOKE_MSG);
                                        response.Difference = -1;
                                        response.Response = new Score() { DownCount = (int)comment.DownCount, UpCount = (int)comment.UpCount };

                                    }
                                    break;
                                case -1:
                                    //change upvote to downvote


                                    if (existingVoteTracker != null)
                                    {

                                        comment.UpCount--;
                                        comment.DownCount++;

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;
                                        _db.SaveChanges();

                                        response = VoteResponse.Success(vote);
                                        response.Difference = -2;
                                        response.Response = new Score() { DownCount = (int)comment.DownCount, UpCount = (int)comment.UpCount };
                                    }
                                    break;
                            }
                            break;
                        case -1: //Previous downvote

                            switch (vote)
                            {
                                case 0: //revoke
                                case -1: //revote which means revoke

                                    if (existingVoteTracker != null)
                                    {
                                        comment.DownCount--;
                                        _db.CommentVoteTrackers.Remove(existingVoteTracker);
                                        _db.SaveChanges();
                                        response = VoteResponse.Success(0, REVOKE_MSG);
                                        response.Difference = 1;
                                        response.Response = new Score() { DownCount = (int)comment.DownCount, UpCount = (int)comment.UpCount };
                                    }
                                    break;
                                case 1:

                                    //change downvote to upvote
                                    if (existingVoteTracker != null)
                                    {
                                        comment.UpCount++;
                                        comment.DownCount--;

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;

                                        _db.SaveChanges();
                                        response = VoteResponse.Success(vote);
                                        response.Difference = 2;
                                        response.Response = new Score() { DownCount = (int)comment.DownCount, UpCount = (int)comment.UpCount };
                                    }

                                    break;
                            }
                            break;
                    }
                    //Set owner user name for notifications
                    response.OwnerUserName = comment.UserName;
                    return response;
                }
            }
            return VoteResponse.Denied();
        }

        [Authorize]
        public VoteResponse VoteSubmission(int submissionID, int vote, string addressHash, bool revokeOnRevote = true)
        {
            DemandAuthentication();

            //make sure we don't have bad int values for vote
            if (Math.Abs(vote) > 1)
            {
                throw new ArgumentOutOfRangeException("vote", "Valid values for vote are only: -1, 0, 1");
            }

            string userName = User.Identity.Name;
            var ruleContext = new VoatRuleContext();
            ruleContext.PropertyBag.AddressHash = addressHash;
            RuleOutcome outcome = null;

            string REVOKE_MSG = "Vote has been revoked";
            Data.Models.Submission submission = null;
            var synclock_submission = _lockStore.GetLockObject(String.Format("submission:{0}", submissionID));
            lock (synclock_submission)
            {
                submission = _db.Submissions.FirstOrDefault(x => x.ID == submissionID);

                if (submission != null)
                {
                    ruleContext.SubmissionID = submissionID;

                    switch (vote)
                    {
                        case 1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.UpVote, RuleScope.VoteSubmission, RuleScope.UpVoteSubmission);
                            if (outcome.IsDenied)
                            {
                                return VoteResponse.Create(outcome);
                            }
                            break;
                        case -1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.DownVote, RuleScope.VoteSubmission, RuleScope.DownVoteSubmission);
                            if (outcome.IsDenied)
                            {
                                return VoteResponse.Create(outcome);
                            }
                            break;
                    }

                    // do not execute voting, subverse is in anonymized mode
                    if (submission.IsAnonymized)
                    {
                        return VoteResponse.Ignored(0, "Subverse is anonymized, voting disabled");
                    }

                    //ignore votes if comment is users
                    if (String.Equals(submission.UserName, userName, StringComparison.OrdinalIgnoreCase))
                    {
                        return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    var existingVote = 0;

                    var existingVoteTracker = _db.SubmissionVoteTrackers.FirstOrDefault(x => x.SubmissionID == submissionID && x.UserName == userName);

                    if (existingVoteTracker != null && existingVoteTracker.VoteStatus.HasValue)
                    {
                        existingVote = existingVoteTracker.VoteStatus.Value;
                    }

                    // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                    if (existingVote == vote && !revokeOnRevote)
                    {
                        return VoteResponse.Ignored(existingVote, "User has already voted this way");
                    }
                    VoteResponse response = new VoteResponse(Status.NotProcessed, 0, "Vote not processed.");


                    switch (existingVote)
                    {

                        case 0: //Never voted or No vote

                            switch (vote)
                            {
                                case 0: //revoke

                                    response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");

                                    break;
                                case 1:
                                case -1:

                                    if (vote == 1)
                                    {
                                        submission.UpCount++;
                                    }
                                    else
                                    {
                                        submission.DownCount++;
                                    }

                                    var t = new SubmissionVoteTracker
                                    {
                                        SubmissionID = submissionID,
                                        UserName = userName,
                                        VoteStatus = vote,
                                        IPAddress = addressHash,
                                        CreationDate = Repository.CurrentDate
                                    };

                                    _db.SubmissionVoteTrackers.Add(t);
                                    _db.SaveChanges();

                                    response = VoteResponse.Success(vote);
                                    response.Difference = vote;
                                    response.Response = new Score() { DownCount = (int)submission.DownCount, UpCount = (int)submission.UpCount };
                                    break;
                            }
                            break;
                        case 1: //Previous Upvote

                            switch (vote)
                            {
                                case 0: //revoke
                                case 1: //revote which means revoke if we are here

                                    if (existingVoteTracker != null)
                                    {
                                        submission.UpCount--;
                                        _db.SubmissionVoteTrackers.Remove(existingVoteTracker);
                                        _db.SaveChanges();

                                        response = response = VoteResponse.Success(0, REVOKE_MSG);
                                        response.Difference = -1;
                                        response.Response = new Score() { DownCount = (int)submission.DownCount, UpCount = (int)submission.UpCount };
                                    }
                                    break;
                                case -1:
                                    //change upvote to downvote

                                    if (existingVoteTracker != null)
                                    {

                                        submission.UpCount--;
                                        submission.DownCount++;

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;

                                        _db.SaveChanges();

                                        response = VoteResponse.Success(vote);
                                        response.Difference = -2;
                                        response.Response = new Score() { DownCount = (int)submission.DownCount, UpCount = (int)submission.UpCount };
                                    }
                                    break;
                            }
                            break;
                        case -1: //Previous downvote
                            switch (vote)
                            {
                                case 0: //revoke
                                case -1: //revote which means revoke if we are here
                                         // delete existing downvote

                                    if (existingVoteTracker != null)
                                    {
                                        submission.DownCount--;
                                        _db.SubmissionVoteTrackers.Remove(existingVoteTracker);
                                        _db.SaveChanges();

                                        response = VoteResponse.Success(0, REVOKE_MSG);
                                        response.Difference = 1;
                                        response.Response = new Score() { DownCount = (int)submission.DownCount, UpCount = (int)submission.UpCount };
                                    }
                                    break;
                                case 1:
                                    //change downvote to upvote
                                    if (existingVoteTracker != null)
                                    {
                                        submission.UpCount++;
                                        submission.DownCount--;

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;

                                        _db.SaveChanges();
                                        response = VoteResponse.Success(vote);
                                        response.Difference = 2;
                                        response.Response = new Score() { DownCount = (int)submission.DownCount, UpCount = (int)submission.UpCount };
                                    }
                                    break;
                            }
                            break;
                    }
                    //Set owner user name for notifications
                    response.OwnerUserName = submission.UserName;
                    return response;
                }
                return VoteResponse.Denied();
            }
        }
        #endregion

        #region Subverse

        public IEnumerable<SubverseInformation> GetDefaultSubverses()
        {

            var defaults = (from d in _db.DefaultSubverses
                            join x in _db.Subverses on d.Subverse equals x.Name
                            orderby d.Order
                            select new SubverseInformation
                            {
                                Name = x.Name,
                                SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                                CreationDate = x.CreationDate,
                                Description = x.Description,
                                RatedAdult = x.IsAdult,
                                Title = x.Title,
                                Type = x.Type,
                                Sidebar = x.SideBar
                            }).ToList();
            return defaults;
        }

        public IEnumerable<SubverseInformation> GetTopSubscribedSubverses(int count = 200)
        {
            var subs = (from x in _db.Subverses
                        orderby x.SubscriberCount descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            RatedAdult = x.IsAdult,
                            Title = x.Title,
                            Type = x.Type,
                            Sidebar = x.SideBar
                        }).Take(count).ToList();
            return subs;
        }

        public IEnumerable<SubverseInformation> GetNewestSubverses(int count = 100)
        {
            var subs = (from x in _db.Subverses
                        orderby x.CreationDate descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            RatedAdult = x.IsAdult,
                            Title = x.Title,
                            Type = x.Type,
                            Sidebar = x.SideBar
                        }
                        ).Take(count).ToList();
            return subs;
        }

        public IEnumerable<SubverseInformation> FindSubverses(string phrase, int count = 50)
        {
            var subs = (from x in _db.Subverses
                        where x.Name.Contains(phrase) || x.Description.Contains(phrase)
                        orderby x.SubscriberCount descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            RatedAdult = x.IsAdult,
                            Title = x.Title,
                            Type = x.Type,
                            Sidebar = x.SideBar
                        }
                        ).Take(count).ToList();
            return subs;
        }

        public Subverse GetSubverseInfo(string subverse)
        {

            var submission = (from x in _db.Subverses
                              where x.Name == subverse
                              select x).FirstOrDefault();

            return submission;
        }
        public string GetSubverseStylesheet(string subverse)
        {

            var sheet = (from x in _db.Subverses
                         where x.Name.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                         select x.Stylesheet).FirstOrDefault();
            return String.IsNullOrEmpty(sheet) ? "" : sheet;
        }
        public IEnumerable<SubverseModerator> GetSubverseModerators(string subverse)
        {
            var data = (from x in _db.SubverseModerators
                        where x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        orderby x.CreationDate ascending
                        select x).ToList();

            return data.AsEnumerable();
        }
        public IEnumerable<SubverseModerator> GetSubversesUserModerates(string userName)
        {
            var data = (from x in _db.SubverseModerators
                        where x.UserName == userName
                        select x).ToList();

            return data.AsEnumerable();
        }

        #endregion

        #region Submissions

        public IEnumerable<Data.Models.Submission> GetTopViewedSubmissions()
        {
            var startDate = CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var data = (from submission in _db.Submissions
                        join subverse in _db.Subverses on submission.Subverse equals subverse.Name
                        where !submission.IsArchived && !submission.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && submission.CreationDate >= startDate && submission.CreationDate <= CurrentDate
                        where !(from bu in _db.BannedUsers select bu.UserName).Contains(submission.UserName)
                        where !subverse.IsAdminDisabled.Value
                        //where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                        orderby submission.Views descending
                        select submission).Take(5).ToList();
            return data.AsEnumerable();
        }

        public string SubverseForSubmission(int submissionID)
        {
            var subname = (from x in _db.Submissions
                           where x.ID == submissionID
                           select x.Subverse).FirstOrDefault();
            return subname;
        }

        public Models.Submission GetSubmission(int submissionID)
        {

            var query = (from x in _db.Submissions
                         where x.ID == submissionID
                         select x);

            var record = query.Select(Selectors.SecureSubmission).FirstOrDefault();

            return record;

        }
        public IEnumerable<Models.Submission> GetUserSubmissions(string subverse, string userName, SearchOptions options)
        {

            //This is a near copy of GetSubmissions<T> 
            if (String.IsNullOrEmpty(userName))
            {
                throw new VoatValidationException("A username must be provided.");
            }
            if (!String.IsNullOrEmpty(subverse) && !SubverseExists(subverse))
            {
                throw new VoatValidationException("Subverse '{0}' doesn't exist.", subverse);
            }
            if (!UserHelper.UserExists(userName))
            {
                throw new VoatValidationException("User does not exist.");
            }

            IQueryable<Models.Submission> query;

            subverse = ToCorrectSubverseCasing(subverse);

            query = (from x in _db.Submissions
                     where (x.UserName == userName && !x.IsAnonymized && !x.IsDeleted)
                     && (x.Subverse == subverse || subverse == null)
                     select x);

            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = query.Select(Selectors.SecureSubmission).ToList();

            return results;

        }
       
        public IEnumerable<Models.Submission> GetSubmissions(string subverse, SearchOptions options)
        {

            if (String.IsNullOrEmpty(subverse))
            {
                throw new VoatValidationException("A subverse must be provided.");
            }

            if (options == null)
            {
                options = new SearchOptions();
            }


            IQueryable<Models.Submission> query;

            switch (subverse.ToLower())
            {

                //for *special* subverses, this is UNDONE
                case AGGREGATE_SUBVERSE.FRONT:
                    if (User.Identity.IsAuthenticated && UserHelper.SubscriptionCount(User.Identity.Name) > 0)
                    {
                        query = (from x in _db.Submissions
                                 join subscribed in _db.SubverseSubscriptions on x.Subverse equals subscribed.Subverse
                                 where subscribed.UserName == User.Identity.Name
                                 select x);
                    }
                    else
                    {
                        //if no user, default to default
                        query = (from x in _db.Submissions
                                 join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
                                 select x);
                    }
                    break;

                case AGGREGATE_SUBVERSE.DEFAULT:

                    query = (from x in _db.Submissions
                             join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
                             select x);
                    break;
                case AGGREGATE_SUBVERSE.ANY:

                    query = (from x in _db.Submissions
                             where !x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate
                             select x);
                    break;
                case AGGREGATE_SUBVERSE.ALL:
                case "all":

                    var nsfw = (User.Identity.IsAuthenticated ? UserHelper.AdultContentEnabled(User.Identity.Name) : false);

                    //v/all has certain conditions
                    //1. Only subs that have a MinCCP of zero
                    //2. Don't show private subs
                    //3. Don't show NSFW subs if nsfw isn't enabled in profile, if they are logged in 
                    //4. Don't show blocked subs if logged in // not implemented

                    query = (from x in _db.Submissions
                             where x.Subverse1.MinCCPForDownvote == 0
                                    && (!x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate)
                                    && (x.Subverse1.IsAdult && nsfw || !x.Subverse1.IsAdult)
                             select x);

                    break;
                //for regular subverse queries
                default:

                    if (!SubverseExists(subverse))
                    {
                        throw new VoatNotFoundException("Subverse '{0}' not found.", subverse);
                    }

                    subverse = ToCorrectSubverseCasing(subverse);

                    query = (from x in _db.Submissions
                             where (x.Subverse == subverse || subverse == null)
                             select x);
                    break;
            }

            query = query.Where(x => !x.IsDeleted);

            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = query.Select(Selectors.SecureSubmission).ToList();

            return results.AsEnumerable();

        }

        [Authorize]
        public CommandResponse<Models.Submission> PostSubmission(string subverse, UserSubmission submission)
        {
            DemandAuthentication();

            //Validation stuff
            if (submission.Title.Equals(submission.Url, StringComparison.InvariantCultureIgnoreCase))
            {
                return CommandResponse.Denied<Models.Submission>(null, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
            }
            if (submission == null || !submission.HasState)
            {
                return CommandResponse.Denied<Models.Submission>(null, "The submission must not be null or have invalid state.");
            }
            if (String.IsNullOrEmpty(subverse))
            {
                throw new VoatValidationException("A subverse must be provided.");
                return CommandResponse.Denied<Models.Submission>(null, "A subverse must be provided.");
            }
            if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content))
            {
                return CommandResponse.Denied<Models.Submission>(null, "Either a Url or Content must be provided.");
            }
            if (String.IsNullOrEmpty(submission.Title))
            {
                return CommandResponse.Denied<Models.Submission>(null, "Submission must have a title.");
            }
            if (Submissions.ContainsUnicode(submission.Title))
            {
                return CommandResponse.Denied<Models.Submission>(null, "Submission title can not contain Unicode characters.");
            }
            if (!SubverseExists(subverse) || subverse.Equals("all", StringComparison.OrdinalIgnoreCase)) //<-- the all subverse actually exists? HA!
            {
                return CommandResponse.Denied<Models.Submission>(null, "Subverse does not exist.");
            }

            //Load Subverse Object
            var cmdSubverse = new QuerySubverse(subverse);
            var subverseObject = cmdSubverse.ExecuteAsync().Result;

            //Evaluate Rules
            var context = new VoatRuleContext();
            context.Subverse = subverseObject;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.PostSubmission, true);

            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Models.Submission>(outcome, null);
            }

            //Check for banned domain content
            var containsBannedDomain = BanningUtility.ContentContainsBannedDomain(subverse, submission.Content);
            if (containsBannedDomain)
            {
                return CommandResponse.Denied<Models.Submission>(null, "Sorry, this post contains links to banned domains.");
            }

            //Save submission
            Models.Submission m = new Models.Submission();
            m.UpCount = 1; //https://voat.co/v/PreviewAPI/comments/877596
            m.UserName = User.Identity.Name;
            m.CreationDate = CurrentDate;
            m.Subverse = subverseObject.Name; 

            //1: Self Post, 2: Link Post
            m.Type = (String.IsNullOrEmpty(submission.Url) ? 1 : 2);

            if (m.Type == 1)
            {
                m.Title = submission.Title;
                m.Content = submission.Content;
                m.LinkDescription = null;
            }
            else
            {
                m.Title = null;
                m.Content = submission.Url;
                m.LinkDescription = submission.Title;
            }

            _db.Submissions.Add(m);

            _db.SaveChanges();

            return CommandResponse.Success(Selectors.SecureSubmission(m));
        }

        [Authorize]
        public Models.Submission EditSubmission(int submissionID, UserSubmission submission)
        {

            if (submission == null || !submission.HasState)
            {
                throw new VoatValidationException("The submission must not be null or have invalid state.");
            }

            //if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content)) {
            //    throw new VoatValidationException("Either a Url or Content must be provided.");
            //}

            var m = GetSubmission(submissionID);

            if (m == null)
            {
                throw new VoatNotFoundException(String.Format("Can't find submission with ID {0}", submissionID));
            }

            if (m.UserName != User.Identity.Name)
            {
                throw new VoatSecurityException(String.Format("Submission can not be edited by account"));
            }

            //only allow edits for self posts
            if (m.Type == 1)
            {
                m.Content = submission.Content ?? m.Content;
            }

            //allow edit of title if in 10 minute window
            if (CurrentDate.Subtract(m.CreationDate).TotalMinutes <= 10.0f)
            {

                if (!String.IsNullOrEmpty(submission.Title) && Utilities.Submissions.ContainsUnicode(submission.Title))
                {
                    throw new VoatValidationException("Submission title can not contain Unicode characters.");
                }

                if (m.Type == 1)
                {
                    m.Title = (String.IsNullOrEmpty(submission.Title) ? m.Title : submission.Title);
                }
                else {
                    m.LinkDescription = (String.IsNullOrEmpty(submission.Title) ? m.LinkDescription : submission.Title);
                }
            }

            m.LastEditDate = CurrentDate;

            _db.SaveChanges();

            return Selectors.SecureSubmission(m);
        }

        [Authorize]
        //LOGIC COPIED FROM SubmissionController.DeleteSubmission(int)
        public Models.Submission DeleteSubmission(int submissionID)
        {
            DemandAuthentication();

            var submissionToDelete = _db.Submissions.Find(submissionID);

            if (submissionToDelete != null)
            {
                // delete submission if delete request is issued by submission author
                if (submissionToDelete.UserName == User.Identity.Name)
                {
                    submissionToDelete.IsDeleted = true;

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.Content = "deleted by author at " + Repository.CurrentDate;
                    }
                    else {
                        submissionToDelete.Content = "http://voat.co";
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionID);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmissions.Remove(existingSticky);
                    }

                    _db.SaveChanges();
                }

                // delete submission if delete request is issued by subverse moderator
                else if (IsUserModerator(submissionToDelete.Subverse))
                {
                    // mark submission as deleted
                    submissionToDelete.IsDeleted = true;

                    // move the submission to removal log
                    var removalLog = new SubmissionRemovalLog
                    {
                        SubmissionID = submissionToDelete.ID,
                        Moderator = User.Identity.Name,
                        Reason = "This feature is not yet implemented",
                        CreationDate = Repository.CurrentDate
                    };

                    _db.SubmissionRemovalLogs.Add(removalLog);

                    // notify submission author that his submission has been deleted by a moderator
                    var message =
                        "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.ID + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " at " + Repository.CurrentDate + "  " + Environment.NewLine +
                        "Original submission content was: " + Environment.NewLine + "---" + Environment.NewLine +
                        (submissionToDelete.Type == 1 ?
                            "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                            "Submission content: " + submissionToDelete.Content
                        :
                            "Link description: " + submissionToDelete.LinkDescription + ", " + Environment.NewLine +
                            "Link URL: " + submissionToDelete.Content
                        );

                    MesssagingUtility.SendPrivateMessage(
                           "Voat",
                           submissionToDelete.UserName,
                           "Your submission has been deleted by a moderator",
                           message
                    );


                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionID);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmissions.Remove(existingSticky);
                    }

                    _db.SaveChanges();
                }
                else {
                    throw new VoatSecurityException("User doesn't have permission to delete submission.");
                }
            }
            return Selectors.SecureSubmission(submissionToDelete);
        }

        #endregion

        #region Comments 

        public IEnumerable<Models.Comment> GetUserComments(string userName, SearchOptions options)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new VoatValidationException("A user name must be provided.");
            }
            if (!UserHelper.UserExists(userName))
            {
                throw new VoatValidationException("User '{0}' does not exist.", userName);
            }

            var query = (from x in _db.Comments
                         where
                            (x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                            && !x.IsAnonymized && !x.IsDeleted
                         select x);

            query = ApplyCommentSearch(options, query);

            //execute query
            var results = query.Select(Selectors.SecureComment).ToList();

            return results;
        }

        public IEnumerable<Models.Comment> GetComments(int? submissionID, SearchOptions options)
        {
            var query = (from x in _db.Comments
                         where
                            (!x.Submission.Subverse1.IsPrivate && submissionID == null) && //got to filter out comments from private subs if calling from the streaming endpoint
                            (x.SubmissionID == submissionID || submissionID == null)
                         select x);

            query = ApplyCommentSearch(options, query);

            //execute query
            var results = query.Select(Selectors.SecureComment).ToList();

            return results;
        }

        //This is the new process to retrieve comments.
        public IEnumerable<usp_CommentTree_Result> GetCommentTree(int submissionID, int? depth, int? parentID)
        {

            if (depth.HasValue && depth < 0)
            {
                depth = null;
            }

            var commentTree = _db.usp_CommentTree(submissionID, depth, parentID);

            //execute query
            var results = commentTree.Select(Selectors.SecureCommentTree).ToList();

            return results;
        }

        public Models.Comment GetComment(int commentID)
        {

            var direct = _db.Comments.Where(x => x.ID == commentID);

            var record = direct.Select(Selectors.SecureComment).FirstOrDefault();

            return record;

        }
        
        [Authorize]
        public Models.Comment DeleteComment(int commentID)
        {
            DemandAuthentication();

            var comment = _db.Comments.Find(commentID);

            if (comment != null)
            {
                var q = new QuerySubmission(comment.SubmissionID.Value);
                var submission = Task.Run(() => q.ExecuteAsync()).Result;

                var commentSubverse = submission.Subverse;
                // delete comment if the comment author is currently logged in user
                if (comment.UserName == User.Identity.Name)
                {
                    comment.IsDeleted = true;
                    comment.Content = "deleted by author at " + CurrentDate.ToLongDateString();
                }
                else
                {
                    if (IsUserModerator(commentSubverse))
                    {
                        comment.IsDeleted = true;
                        comment.Content = "deleted by moderator at " + CurrentDate;

                        // notify comment author that his comment has been deleted by a moderator
                        MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            comment.UserName,
                            "Your comment has been deleted by a moderator",
                            "Your [comment](/v/" + commentSubverse + "/comments/" + comment.SubmissionID + "/" + comment.ID + ") has been deleted by: " +
                            "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " on: " + CurrentDate + "  " + Environment.NewLine +
                            "Original comment content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            comment.Content
                            );
                    }
                    else
                    {
                        var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                        ex.Data["CommentID"] = commentID;
                        throw ex;
                    }
                }
                _db.SaveChanges();
            }
            return Selectors.SecureComment(comment);
        }

        [Authorize]
        public Models.Comment EditComment(int commentID, string comment)
        {
            var current = _db.Comments.Find(commentID);

            if (current != null)
            {

                if (current.UserName.Trim() == User.Identity.Name)
                {

                    current.LastEditDate = CurrentDate;

                    var escapedCommentContent = WebUtility.HtmlEncode(comment);
                    current.Content = escapedCommentContent;

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                    {
                        current.Content = ContentProcessor.Instance.Process(current.Content, ProcessingStage.InboundPreSave, current);
                    }

                    _db.SaveChanges();

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                    {
                        ContentProcessor.Instance.Process(current.Content, ProcessingStage.InboundPostSave, current);
                    }

                }
                else {
                    var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                    ex.Data["UserName"] = User.Identity.Name;
                    ex.Data["CommentID"] = commentID;
                    throw ex;
                }
            }
            else {
                throw new VoatNotFoundException("Can not find comment with ID {0}", commentID);
            }

            return Selectors.SecureComment(current);

        }

        [Authorize]
        public CommandResponse<Models.Comment> PostCommentReply(int parentCommentID, string comment)
        {
            var c = _db.Comments.Find(parentCommentID);
            if (c == null)
            {
                throw new VoatNotFoundException("Can not find parent comment with id {0}", parentCommentID.ToString());
            }
            var submissionid = c.SubmissionID;
            return PostComment(submissionid.Value, parentCommentID, comment);
        }

        [Authorize]
        public CommandResponse<Models.Comment> PostComment(int submissionID, int? parentCommentID, string comment)
        {

            DemandAuthentication();

            var submission = _db.Submissions.Find(submissionID);

            if (submission == null)
            {
                throw new VoatNotFoundException("submissionID", submissionID, "Can not find submission");
            }

            //Check for banned domain content
            var containsBannedDomain = BanningUtility.ContentContainsBannedDomain(submission.Subverse, comment);
            if (containsBannedDomain)
            {
                return CommandResponse.Denied<Models.Comment>(null, "Sorry, this comment contains links to banned domains.");
            }

            var subverse = _db.Subverses.Where(x => x.Name == submission.Subverse).FirstOrDefault();
            var c = new Models.Comment();
            c.CreationDate = Repository.CurrentDate;
            c.UserName = User.Identity.Name;
            c.ParentID = (parentCommentID > 0 ? parentCommentID : (int?)null);
            c.SubmissionID = submissionID;
            c.Votes = 0;
            c.UpCount = 0;
            c.IsAnonymized = (submission.IsAnonymized || subverse.IsAnonymized);

            c.Content = ContentProcessor.Instance.Process(comment, ProcessingStage.InboundPreSave, c);
            //save fully formatted content 
            var formattedComment = Formatting.FormatMessage(c.Content);
            c.FormattedContent = formattedComment;

            //evaluate rule
            VoatRuleContext context = new VoatRuleContext();
            //set any state we have so context doesn't have to retrieve
            context.SubmissionID = submissionID;
            context.Subverse = subverse;
            context.PropertyBag.Submission = submission;

            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.PostComment);

            if (outcome.IsAllowed)
            {
                _db.Comments.Add(c);
                _db.SaveChanges();
            }

            if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            {
                ContentProcessor.Instance.Process(c.Content, ProcessingStage.InboundPostSave, c);
            }

            return MapRuleOutCome(outcome, Selectors.SecureComment(c));

        }

        #endregion

        #region Api

        public bool IsApiKeyValid(string apiPublicKey)
        {

            var key = GetApiKey(apiPublicKey);

            if (key != null && key.IsActive)
            {

                //TODO: This needs to be non-blocking and non-queued. If 20 threads with same apikey are accessing this method at once we don't want to perform 20 updates on record.  
                //keep track of last access date
                key.LastAccessDate = CurrentDate;
                _db.SaveChanges();

                return true;
            }

            return false;
        }

        public ApiClient GetApiKey(string apiPublicKey)
        {

            var result = (from x in this._db.ApiClients
                          where x.PublicKey == apiPublicKey
                          select x).FirstOrDefault();
            return result;
        }

        [Authorize]
        public IEnumerable<ApiClient> GetApiKeys(string userName)
        {

            var result = from x in this._db.ApiClients
                         where x.UserName == userName
                         orderby x.CreationDate descending
                         select x;
            return result.ToList();
        }

        [Authorize]
        public ApiThrottlePolicy GetApiThrottlePolicy(int throttlePolicyID)
        {
            var result = from policy in _db.ApiThrottlePolicies
                         where policy.ID == throttlePolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public ApiPermissionPolicy GetApiPermissionPolicy(int permissionPolicyID)
        {
            var result = from policy in _db.ApiPermissionPolicies
                         where policy.ID == permissionPolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public List<KeyValuePair<string, string>> GetApiClientKeyThrottlePolicies()
        {

            List<KeyValuePair<string, string>> policies = new List<KeyValuePair<string, string>>();

            var result = from client in this._db.ApiClients
                         join policy in _db.ApiThrottlePolicies on client.ApiThrottlePolicyID equals policy.ID
                         where client.IsActive == true
                         select new { client.PublicKey, policy.Policy };

            foreach (var policy in result)
            {
                policies.Add(new KeyValuePair<string, string>(policy.PublicKey, policy.Policy));
            }

            return policies;
        }

        [Authorize]
        public void CreateApiKey(string name, string description, string url, string redirectUrl)
        {

            ApiClient c = new ApiClient();
            c.IsActive = true;
            c.AppAboutUrl = url;
            c.RedirectUrl = redirectUrl;
            c.AppDescription = description;
            c.AppName = name;
            c.UserName = User.Identity.Name;
            c.CreationDate = CurrentDate;

            var generatePublicKey = new Func<string>(() => {
                return String.Format("VO{0}AT", Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            });

            //just make sure key isn't already in db
            var publicKey = generatePublicKey();
            while (_db.ApiClients.Any(x => x.PublicKey == publicKey))
            {
                publicKey = generatePublicKey();
            }

            c.PublicKey = publicKey;
            c.PrivateKey = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-", "").ToUpper();

            //Using OAuth 2, we don't need enc keys
            //var keyGen = RandomNumberGenerator.Create();
            //byte[] tempKey = new byte[16];
            //keyGen.GetBytes(tempKey);
            //c.PublicKey = Convert.ToBase64String(tempKey);

            //tempKey = new byte[64];
            //keyGen.GetBytes(tempKey);
            //c.PrivateKey = Convert.ToBase64String(tempKey);

            _db.ApiClients.Add(c);
            _db.SaveChanges();

        }

        [Authorize]
        public ApiClient DeleteApiKey(int id)
        {
            //Only allow users to delete ApiKeys if they IsActive == 1
            var apiKey = (from x in _db.ApiClients
                          where x.ID == id && x.UserName == User.Identity.Name && x.IsActive == true
                          select x).FirstOrDefault();

            if (apiKey != null)
            {
                _db.ApiClients.Remove(apiKey);
                _db.SaveChanges();
            }
            return apiKey;
        }
        public IEnumerable<ApiCorsPolicy> GetApiCorsPolicies()
        {
            var policy = (from x in _db.ApiCorsPolicies
                          where
                          x.IsActive
                          select x).ToList();
            return policy;
        }
        public ApiCorsPolicy GetApiCorsPolicy(string origin)
        {
            var domain = origin;

            //Match and pull domain only
            var domainMatch = Regex.Match(origin, @"://(?<domainPort>(?<domain>[\w.-]+)(?::\d+)?)[/]?");
            if (domainMatch.Success)
            {
                domain = domainMatch.Groups["domain"].Value;
                //var domain = domainMatch.Groups["domainPort"];
            }

            var policy = (from x in _db.ApiCorsPolicies
                          //haven't decided exactly how we are going to store origin (i.e. just the doamin name, with/without protocol, etc.)
                          where 
                          (x.AllowOrigin.Equals(origin, StringComparison.OrdinalIgnoreCase) 
                          || x.AllowOrigin.Equals(domain, StringComparison.OrdinalIgnoreCase))
                          && x.IsActive
                          select x).FirstOrDefault();
            return policy;
        }

        public void SaveApiLogEntry(ApiLog logentry)
        {
            logentry.CreationDate = CurrentDate;
            _db.ApiLogs.Add(logentry);
            _db.SaveChanges();
        }

        public void UpdateApiClientLastAccessDate(int apiClientID)
        {
            var client = _db.ApiClients.Where(x => x.ID == apiClientID).FirstOrDefault();
            client.LastAccessDate = CurrentDate;
            _db.SaveChanges();
        }

        #endregion

        #region UserMessages
        /// <summary>
        /// Save Comments and Submissions toggle.
        /// </summary>
        /// <param name="type">The type of content in which to save</param>
        /// <param name="ID">The ID of the item in which to save</param>
        /// <param name="forceAction">Forces the Save function to operate as a Save only or Unsave only rather than a toggle. If true, will only save if it hasn't been previously saved, if false, will only remove previous saved entry, if null (default) will function as a toggle.</param>
        /// <returns>The end result if the item is saved or not. True if saved, false if not saved.</returns>
        public bool Save(ContentType type, int ID, bool? forceAction = null)
        {

            //TODO: These save trackers should be stored in a single table in SQL. Two tables for such similar information isn't ideal... mmkay. Makes querying nasty. 
            //TODO: There is a potential issue with this code. There is no validation that the ID belongs to a comment or a submission. This is nearly impossible to determine anyways but it's still an issue.
            string currentUserName = User.Identity.Name;
            bool isSaved = false;

            switch (type)
            {
                case ContentType.Comment:

                    var c = _db.CommentSaveTrackers.FirstOrDefault(x => x.CommentID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));

                    if (c == null && (forceAction == null || forceAction.HasValue && forceAction.Value))
                    {

                        c = new CommentSaveTracker() { CommentID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.CommentSaveTrackers.Add(c);
                        isSaved = true;

                    }
                    else if (c != null && (forceAction == null || forceAction.HasValue && !forceAction.Value))
                    {

                        _db.CommentSaveTrackers.Remove(c);
                        isSaved = false;

                    }
                    _db.SaveChanges();

                    break;
                case ContentType.Submission:

                    var s = _db.SubmissionSaveTrackers.FirstOrDefault(x => x.SubmissionID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    if (s == null && (forceAction == null || forceAction.HasValue && forceAction.Value))
                    {

                        s = new SubmissionSaveTracker() { SubmissionID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.SubmissionSaveTrackers.Add(s);
                        isSaved = true;

                    }
                    else if (s != null && (forceAction == null || forceAction.HasValue && !forceAction.Value))
                    {

                        _db.SubmissionSaveTrackers.Remove(s);
                        isSaved = false;

                    }
                    _db.SaveChanges();

                    break;
            }

            return (forceAction.HasValue ? forceAction.Value : isSaved);

        }

        [Authorize]
        public void SaveUserPrefernces(UserPreference preferences)
        {
            DemandAuthentication();

            var p = (from x in _db.UserPreferences
                     where x.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();

            if (p == null)
            {
                p = new UserPreference();
                p.UserName = User.Identity.Name;
            }

            if (!String.IsNullOrEmpty(preferences.Avatar))
            {
                p.Avatar = preferences.Avatar;
            }
            if (!String.IsNullOrEmpty(preferences.Bio))
            {
                p.Bio = preferences.Bio;
            }
            if (!String.IsNullOrEmpty(preferences.Language))
            {
                p.Language = preferences.Language;
            }
            p.OpenInNewWindow = preferences.OpenInNewWindow;
            p.DisableCSS = preferences.DisableCSS;
            p.EnableAdultContent = preferences.EnableAdultContent;
            p.NightMode = preferences.NightMode;
            p.DisplaySubscriptions = preferences.DisplaySubscriptions;
            p.DisplayVotes = preferences.DisplayVotes;
            p.UseSubscriptionsMenu = preferences.UseSubscriptionsMenu;

            _db.UserPreferences.Add(p);

            _db.SaveChanges();

        }

        [Authorize]
        public CommandResponse SendMessageReply(int id, string value)
        {
            DemandAuthentication();

            //THIS WILL BE WAY EASIER TO IMPLEMENT AFTER THE MESSAGE TABLES ARE MERGED.
            //find message
            var c = (from x in _db.CommentReplyNotifications
                     where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (c != null)
            {
                PostCommentReply(c.CommentID, value);
                return new CommandResponse(Status.Success, "Comment reply sent");
            }

            var sub = (from x in _db.SubmissionReplyNotifications
                       where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                       select x).FirstOrDefault();
            if (sub != null)
            {
                PostComment(sub.SubmissionID, sub.CommentID, value);
                return new CommandResponse(Status.Success, "Comment reply sent");
            }
            var pm = (from x in _db.PrivateMessages
                      where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                      select x).FirstOrDefault();
            if (pm != null)
            {
                SendMessage(new SendMessage() { Subject = sub.Subject, Message = value, Recipient = sub.Sender });
                return new CommandResponse(Status.Success, "Message sent");
            }
            return new CommandResponse(Status.NotProcessed, "Couldn't find message in which to reply");
        }

        [Authorize]
        public CommandResponse SendMessage(SendMessage message)
        {
            DemandAuthentication();

            if (Voat.Utilities.UserHelper.IsUserGloballyBanned(User.Identity.Name))
            {
                return CommandResponse.Ignored("User is banned");
            }

            if (Voat.Utilities.Karma.CommentKarma(User.Identity.Name) < 10)
            {
                return CommandResponse.Ignored("Comment points too low to send messages", "CCP < 10");
            }

            List<PrivateMessage> messages = new List<PrivateMessage>();
            MatchCollection col = Regex.Matches(message.Recipient, @"((?'prefix'@|u/|/u/|v/|/v/)?(?'recipient'[\w-.]+))", RegexOptions.IgnoreCase);
            if (col.Count <= 0)
            {
                return new CommandResponse(Status.NotProcessed, "No recipient specified");
            }

            //Have to filter distinct because of spamming. If you copy a user name 
            //1,000 times into the recipient list the previous 
            //logic would send that user 1,000 messages. These guys find everything.
            var filtered = (from x in col.Cast<Match>()
                            select new
                            {
                                recipient = x.Groups["recipient"].Value,
                                prefix = (x.Groups["prefix"].Value.ToLower().Contains("v") ? "v" : "") //stop users from sending multiple messages using diff prefixes @user, /u/user, and u/user 
                            }).Distinct();

            foreach (var m in filtered)
            {
                var recipient = m.recipient;
                var prefix = m.prefix;

                //var recipient = m.Groups["recipient"].Value;
                //var prefix = m.Groups["prefix"].Value;

                if (!String.IsNullOrEmpty(prefix) && prefix.ToLower().Contains("v"))
                {
                    //don't allow banned users to send to subverses
                    if (!UserHelper.IsUserBannedFromSubverse(User.Identity.Name, recipient))
                    {
                        //send to subverse mods
                        using (var db = new voatEntities())
                        {
                            //designed to limit abuse by taking the level 1 mod and the next four oldest
                            var mods = (from mod in db.SubverseModerators
                                        where mod.Subverse.Equals(recipient, StringComparison.OrdinalIgnoreCase) && mod.UserName != "system" && mod.UserName != "youcanclaimthissub"
                                        orderby mod.Power ascending, mod.CreationDate descending
                                        select mod).Take(5);

                            foreach (var moderator in mods)
                            {
                                messages.Add(new PrivateMessage
                                {
                                    Sender = User.Identity.Name,
                                    Recipient = moderator.UserName,
                                    CreationDate = Repository.CurrentDate,
                                    Subject = String.Format("[v/{0}] {1}", recipient, message.Subject),
                                    Body = message.Message,
                                    IsUnread = true,
                                    MarkedAsUnread = false
                                });
                            }
                        }
                    }
                }
                else
                {
                    recipient = UserHelper.OriginalUsername(recipient);

                    if (!String.IsNullOrEmpty(recipient) && UserHelper.UserExists(recipient))
                    {
                        //TODO: Check banned user list
                        messages.Add(new PrivateMessage
                        {
                            Sender = User.Identity.Name,
                            Recipient = recipient,
                            CreationDate = Repository.CurrentDate,
                            Subject = message.Subject,
                            Body = message.Message,
                            IsUnread = true,
                            MarkedAsUnread = false
                        });
                    }
                }
            }

            if (messages.Count > 0)
            {
                using (var db = new voatEntities())
                {
                    try
                    {
                        db.PrivateMessages.AddRange(messages);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return CommandResponse.Error<CommandResponse>(ex);
                    }
                }
            }
            return CommandResponse.Success();

        }

        [Authorize]
        public IEnumerable<UserMessage> GetUserMessages(MessageType type, MessageState state, bool markAsRead = true)
        {

            //This ENTIRE routine will need to be refactored once the message tables are merged. THIS SHOULD BE HIGH PRIORITY AS THIS IS HACKY HACKSTERISTIC
            List<UserMessage> messages = new List<UserMessage>();

            if ((type & MessageType.Inbox) > 0 || (type & MessageType.Sent) > 0)
            {
                var msgs = (from x in _db.PrivateMessages
                            where (
                                ((x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type & MessageType.Inbox) > 0) ||
                                (x.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type & MessageType.Sent) > 0)) &&
                                (x.IsUnread == ((state & MessageState.Unread) > 0) || state  == MessageState.All))
                            select new UserMessage()
                            {
                                ID = x.ID,
                                CommentID = null,
                                SubmissionID = null,
                                Subverse = null,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = x.Subject,
                                Content = x.Body,
                                IsRead = !x.IsUnread,
                                Type = (x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) ? MessageType.Inbox : MessageType.Sent),
                                SentDate = x.CreationDate
                            });
                messages.AddRange(msgs.ToList());
            }
            //Comment Replies, Mention Replies
            if ((type & MessageType.Comment) > 0 || (type & MessageType.Mention) > 0)
            {
                var msgs = (from x in _db.CommentReplyNotifications
                            join s in _db.Submissions on x.SubmissionID equals s.ID
                            join c in _db.Comments on x.CommentID equals c.ID into commentJoin
                            from comment in commentJoin.DefaultIfEmpty()
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                                && x.IsUnread == (state == MessageState.Unread)
                                )
                            select new UserMessage()
                            {
                                ID = x.ID,
                                CommentID = (x.CommentID > 0 ? x.CommentID : (int?)null),
                                SubmissionID = x.SubmissionID,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = s.Title,
                                Content = (comment == null ? s.Content : comment.Content),
                                IsRead = !x.IsUnread,
                                Type = (s.Content.Contains(User.Identity.Name) ? MessageType.Mention : MessageType.Comment), //TODO: Need to determine if comment reply or mention
                                SentDate = x.CreationDate
                            });


                if ((type & MessageType.Comment) > 0 && (type & MessageType.Mention) > 0)
                {
                    //this is both
                    messages.AddRange(msgs.ToList());
                }
                else if ((type & MessageType.Mention) > 0)
                {
                    messages.AddRange(msgs.ToList().FindAll(x => x.Type == MessageType.Mention));
                }
                else
                {
                    messages.AddRange(msgs.ToList().FindAll(x => x.Type == MessageType.Comment));
                }
            }

            //Post Replies
            if ((type & MessageType.Submission) > 0)
            {
                var msgs = (from x in _db.SubmissionReplyNotifications
                            join c in _db.Comments on x.CommentID equals c.ID
                            join s in _db.Submissions on c.SubmissionID equals s.ID
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) &&
                                x.IsUnread == (state == MessageState.Unread))
                            select new UserMessage()
                            {
                                ID = x.ID,
                                CommentID = x.CommentID,
                                SubmissionID = x.SubmissionID,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = (s.Type == 1 ? s.LinkDescription : s.Title),
                                Content = c.Content,
                                IsRead = !x.IsUnread,
                                Type = MessageType.Submission,
                                SentDate = x.CreationDate
                            });
                messages.AddRange(msgs.ToList());
            }

            //mark as read, super hacky until message tables are merged
            if (markAsRead)
            {
                new Task(() => {
                    using (var db = new voatEntities())
                    {
                        foreach (var msg in messages)
                        {
                            if (!msg.IsRead)
                            {
                                switch (msg.Type)
                                {
                                    case MessageType.Comment:
                                    case MessageType.Mention:
                                        var m = db.CommentReplyNotifications.First(x => x.ID == msg.ID);
                                        m.IsUnread = false;
                                        break;
                                    case MessageType.Inbox:
                                        var p = db.PrivateMessages.First(x => x.ID == msg.ID);
                                        p.IsUnread = false;

                                        break;
                                    case MessageType.Submission:
                                        var s = db.SubmissionReplyNotifications.First(x => x.ID == msg.ID);
                                        s.IsUnread = false;
                                        break;
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                }).Start();
            }

            return messages.OrderByDescending(x => x.SentDate);
        }

        #endregion

        #region User Related Functions
        public IEnumerable<CommentVoteTracker> UserCommentVotesBySubmission(int submissionID, string userName)
        {
            List<CommentVoteTracker> vCache = new List<CommentVoteTracker>();

            if (!String.IsNullOrEmpty(userName))
            {
                vCache = (from cv in _db.CommentVoteTrackers.AsNoTracking()
                          join c in _db.Comments on cv.CommentID equals c.ID
                          where c.SubmissionID == submissionID && cv.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                          select cv).ToList();
            }
            return vCache;
        }
        public IEnumerable<DomainReference> GetSubscriptions(string userName)
        {


            var subs = (from x in _db.SubverseSubscriptions
                        where x.UserName == userName
                        select new DomainReference() { Name = x.Subverse, Type = DomainType.Subverse }).ToList();

            var sets = (from x in _db.UserSetSubscriptions
                        where x.UserName == userName
                        select new DomainReference() { Name = x.UserSet.Name, Type = DomainType.Set }).ToList();

            subs.AddRange(sets);

            return subs;

        }

        public IList<DomainReference> GetBlockedUsers(string userName)
        {
            var blocked = (from x in _db.UserBlockedUsers
                           where x.UserName == userName
                           select new DomainReference() { Name = x.BlockUser, Type = DomainType.User }).ToList();
            return blocked;
        }

        public IList<DomainReference> GetBlockedSubverses(string userName)
        {
            var blocked = (from x in _db.UserBlockedSubverses
                           where x.UserName == userName
                           select new DomainReference() { Name = x.Subverse, Type = DomainType.Subverse }).ToList();
            return blocked;
        }

        public UserInformation GetUserInfo(string userName)
        {

            //copypasta from LegacyWebApiController.UserInfo
            //This method makes multiple calls to the db, need to reduce overhead eventually
            if (userName != "deleted" && !UserHelper.UserExists(userName) || userName == "deleted")
            {
                return null;
            }

            var userInfo = new UserInformation();


            var userBadges = (from x in _db.UserBadges
                              join b in _db.Badges on x.BadgeID equals b.ID
                              where x.UserName == userName
                              select new Voat.Domain.Models.UserBadge()
                              {
                                  CreationDate = x.CreationDate,
                                  Name = b.Name,
                                  Title = b.Title,
                                  Graphic = b.Graphic,
                              }
                              ).ToList();


            userInfo.Badges = userBadges;

            //Expensive
            userInfo.CommentPoints = UserContributionPoints(userName, ContentType.Comment);
            userInfo.SubmissionPoints = UserContributionPoints(userName, ContentType.Submission);
            userInfo.SubmissionVoting = UserVotingBehavior(userName, ContentType.Submission);
            userInfo.CommentVoting = UserVotingBehavior(userName, ContentType.Comment);

            //TODO: Can do this in one call... get rid of UserHelper calls.
            userInfo.UserName = UserHelper.OriginalUsername(userName);
            userInfo.RegistrationDate = UserHelper.GetUserRegistrationDateTime(userName);
            userInfo.Bio = UserHelper.UserShortbio(userName);
            userInfo.ProfilePicture = VoatPathHelper.AvatarPath(userName, true, true);

            return userInfo;
        }

        public Models.UserPreference GetUserPreferences(string userName)
        {
            var query = _db.UserPreferences.Where(x => (x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)));

            var results = query.FirstOrDefault();

            return results;
        }

        public Score UserVotingBehavior(string userName, ContentType type = ContentType.Comment | ContentType.Submission, TimeSpan? span = null)
        {

            Score vb = new Score();

            if ((type & ContentType.Comment) > 0)
            {
                var c = GetUserVotingBehavior(userName, ContentType.Comment, span);
                vb.Combine(c);
            }
            if ((type & ContentType.Submission) > 0)
            {
                var c = GetUserVotingBehavior(userName, ContentType.Submission, span);
                vb.Combine(c);
            }

            return vb;
        }


        //private Score UserVotingBehaviorSubmissionsEF(string userName, TimeSpan? span = null)
        //{

        //    DateTime? compareDate = null;
        //    if (span.HasValue)
        //    {
        //        compareDate = CurrentDate.Subtract(span.Value);
        //    }


        //    var result = (from x in _db.SubmissionVoteTrackers
        //                  where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
        //                  && ((compareDate.HasValue && x.CreationDate >= compareDate) || !compareDate.HasValue)
        //                  group x by x.VoteStatus into v
        //                  select new
        //                  {
        //                      key = v.Key,
        //                      votes = v.Count()
        //                  });

        //    Score vb = new Score();

        //    if (result != null)
        //    {
        //        foreach (var r in result)
        //        {
        //            if (r.key.HasValue)
        //            {
        //                if (r.key.Value == 1)
        //                {
        //                    vb.UpCount = r.votes;
        //                }
        //                else {
        //                    vb.DownCount = r.votes;
        //                }
        //            }
        //        }
        //    }
        //    return vb;
        //}

        //private Score UserVotingBehaviorCommentsEF(string userName, TimeSpan? span = null)
        //{

        //    DateTime? compareDate = null;
        //    if (span.HasValue)
        //    {
        //        compareDate = CurrentDate.Subtract(span.Value);
        //    }

        //    var result = (from x in _db.CommentVoteTrackers
        //                  where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
        //                   && ((compareDate.HasValue && x.CreationDate >= compareDate) || !compareDate.HasValue)
        //                  group x by x.VoteStatus into v
        //                  select new
        //                  {
        //                      key = v.Key,
        //                      votes = v.Count()
        //                  });

        //    Score vb = new Score();

        //    if (result != null)
        //    {
        //        foreach (var r in result)
        //        {
        //            if (r.key.HasValue)
        //            {
        //                if (r.key.Value == 1)
        //                {
        //                    vb.UpCount = r.votes;
        //                }
        //                else
        //                {
        //                    vb.DownCount = r.votes;
        //                }
        //            }
        //        }
        //    }
        //    return vb;
        //}
        
        private Score GetUserVotingBehavior(string userName, ContentType type, TimeSpan? span = null)
        {
            var score = new Score();

            DateTime? compareDate = null;
            if (span.HasValue)
            {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var cmd = _db.Database.Connection.CreateCommand();
            cmd.CommandText = String.Format(
                                @"SELECT x.VoteStatus, 'Count' = ABS(ISNULL(SUM(x.VoteStatus), 0))
                                FROM {0} x WITH (NOLOCK)
                                WHERE x.UserName = @UserName
                                AND(x.CreationDate >= @CompareDate OR @CompareDate IS NULL)
                                GROUP BY x.VoteStatus", type == ContentType.Comment ? "CommentVoteTracker" : "SubmissionVoteTracker");
            cmd.CommandType = System.Data.CommandType.Text;

            var param = cmd.CreateParameter();
            param.ParameterName = "UserName";
            param.DbType = System.Data.DbType.String;
            param.Value = userName;
            cmd.Parameters.Add(param);

            param = cmd.CreateParameter();
            param.ParameterName = "CompareDate";
            param.DbType = System.Data.DbType.DateTime;
            param.Value = compareDate.HasValue ? compareDate.Value : (object)DBNull.Value;
            cmd.Parameters.Add(param);

            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
            {
                
                while (reader.Read())
                {
                    int voteStatus = (int)reader["VoteStatus"];
                    if (voteStatus == 1)
                    {
                        score.UpCount = (int)reader["Count"];
                    }
                    else if (voteStatus == -1)
                    {
                        score.DownCount = (int)reader["Count"];
                    }
                }
            }
            return score;
        }

        public int UserCommentCount(string userName, TimeSpan? span, string subverse = null)
        {

            DateTime? compareDate = null;
            if (span.HasValue)
            {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.Comments
                          where
                            x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                            && (x.Submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.CreationDate >= compareDate)
                          select x).Count();
            return result;
        }

        public int UserSubmissionCount(string userName, TimeSpan? span, SubmissionType? type = null, string subverse = null)
        {

            DateTime? compareDate = null;
            if (span.HasValue)
            {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.Submissions
                          where
                            x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                            && (x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.CreationDate >= compareDate)
                            && (type != null && x.Type == (int)type.Value) || type == null
                          select x).Count();
            return result;
        }
        public Score UserContributionPoints(string userName, ContentType type, string subverse = null)
        {

            Score s = new Score();

            if ((type & ContentType.Comment) > 0)
            {
                var cmd = _db.Database.Connection.CreateCommand();
                cmd.CommandText = @"SELECT 'UpCount' = CAST(ABS(ISNULL(SUM(c.UpCount),0)) AS INT), 'DownCount' = CAST(ABS(ISNULL(SUM(c.DownCount),0)) AS INT) FROM Comment c WITH (NOLOCK)
                                    INNER JOIN Submission s WITH (NOLOCK) ON(c.SubmissionID = s.ID)
                                    WHERE c.UserName = @UserName
                                    AND (s.Subverse = @Subverse OR @Subverse IS NULL)
                                    AND c.IsAnonymized = 0"; //this prevents anon votes from showing up in stats
                cmd.CommandType = System.Data.CommandType.Text;

                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "Subverse";
                param.DbType = System.Data.DbType.String;
                param.Value = String.IsNullOrEmpty(subverse) ? (object)DBNull.Value : subverse;
                cmd.Parameters.Add(param);

                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        s.Combine(new Score() { UpCount = (int)reader["UpCount"], DownCount = (int)reader["DownCount"] });
                    }
                }
                
            }
            
            if ((type & ContentType.Submission) > 0)
            {
                var cmd = _db.Database.Connection.CreateCommand();
                cmd.CommandText = @"SELECT 'UpCount' = CAST(ABS(ISNULL(SUM(s.UpCount), 0)) AS INT), 'DownCount' = CAST(ABS(ISNULL(SUM(s.DownCount), 0)) AS INT) FROM Submission s WITH (NOLOCK)
                                    WHERE s.UserName = @UserName
                                    AND (s.Subverse = @Subverse OR @Subverse IS NULL)
                                    AND s.IsAnonymized = 0";

                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "Subverse";
                param.DbType = System.Data.DbType.String;
                param.Value = String.IsNullOrEmpty(subverse) ? (object)DBNull.Value : subverse;
                cmd.Parameters.Add(param);

                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        s.Combine(new Score() { UpCount = (int)reader["UpCount"], DownCount = (int)reader["DownCount"] });
                    }
                }
            }

            return s;

        }

        public Score UserContributionPointsEF(string userName, ContentType type, string subverse = null)
        {

            Score s = new Score();

            if ((type & ContentType.Comment) > 0)
            {
                var totals = (from x in _db.Comments
                              where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.UserName into y
                              select new
                              {
                                  up = y.Sum(ups => ups.UpCount),
                                  down = y.Sum(downs => downs.DownCount)
                              }).FirstOrDefault();

                if (totals != null)
                {
                    s.Combine(new Score() { UpCount = (int)totals.up, DownCount = (int)totals.down });
                }
            }

            if ((type & ContentType.Submission) > 0)
            {
                var totals = (from x in _db.Submissions
                              where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.UserName into y
                              select new
                              {
                                  up = y.Sum(ups => ups.UpCount),
                                  down = y.Sum(downs => downs.DownCount)
                              }).FirstOrDefault();
                if (totals != null)
                {
                    s.Combine(new Score() { UpCount = (int)totals.up, DownCount = (int)totals.down });
                }
            }

            return s;

        }
        public void SubscribeUser(DomainType domainType, SubscriptionAction action, string subscriptionName)
        {
            switch (domainType)
            {
                case DomainType.Subverse:

                    if (action == SubscriptionAction.Subscribe)
                    {
                        if (!_db.SubverseSubscriptions.Any(x => x.Subverse.Equals(subscriptionName, StringComparison.OrdinalIgnoreCase) && x.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            var sub = new SubverseSubscription { UserName = User.Identity.Name, Subverse = subscriptionName };
                            _db.SubverseSubscriptions.Add(sub);
                        }
                    }
                    else
                    {
                        var sub = _db.SubverseSubscriptions.FirstOrDefault(x => x.Subverse.Equals(subscriptionName, StringComparison.OrdinalIgnoreCase) && x.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                        if (sub != null)
                        {
                            _db.SubverseSubscriptions.Remove(sub);
                        }
                    }
                    _db.SaveChanges();

                    //TODO: Update Subscriber Count
                    //// record new subscription in subverse table subscribers field
                    //Subverse tmpSubverse = db.Subverses.Find(subverse);
                    //if (tmpSubverse != null)
                    //{
                    //    tmpSubverse.SubscriberCount++;
                    //}
                    //db.SaveChanges();

                    break;
                default:
                    throw new NotImplementedException(String.Format("{0} subscriptions not implemented yet", domainType)); 
                    break;

            }
        }

        #endregion

        #region Admin Functions

        //public void SaveAdminLogEntry(AdminLog log) {

        //    if (log == null){
        //        throw new VoatValidationException("AdminLog can not be null");
        //    }
        //    if (String.IsNullOrEmpty(log.Action)) {
        //        throw new VoatValidationException("AdminLog.Action must have a valid value");
        //    }
        //    if (String.IsNullOrEmpty(log.Type)) {
        //        throw new VoatValidationException("AdminLog.Type must have a valid value");
        //    }
        //    if (String.IsNullOrEmpty(log.Details)) {
        //        throw new VoatValidationException("AdminLog.Details must have a valid value");
        //    }

        //    //Set specific info
        //    log.UserName = User.Identity.Name;
        //    log.CreationDate = CurrentDate;

        //    _db.AdminLogs.Add(log);
        //    _db.SaveChanges();


        //}

        //TODO: Add roles allowed to execute
        //TODO: this method is a multi-set without transaction support. Correct this you hack.
        //[Authorize(Roles="GlobalAdmin,Admin,DelegateAdmin")]
        //public void TransferSubverse(SubverseTransfer transfer) {

        //    if (User.Identity.IsAuthenticated) {

        //        //validate info
        //        string sub = ToCorrectSubverseCasing(transfer.Subverse);
        //        if (String.IsNullOrEmpty(sub) && transfer.IsApproved) {
        //            throw new VoatValidationException("Can not find subverse '{0}'", transfer.Subverse);
        //        }
        //        transfer.Subverse = sub;

        //        string user = ToCorrectUserNameCasing(transfer.UserName);
        //        if (String.IsNullOrEmpty(user)) {
        //            throw new VoatValidationException("Can not find user '{0}'", transfer.UserName);
        //        }
        //        transfer.UserName = user;

        //        if (transfer.IsApproved) {
        //            //Issue transfer // do something with this value later 0 = failed, 1 = success
        //            int success = _db.usp_TransferSubverse(transfer.Subverse, transfer.UserName);
        //        }

        //        //Write Admin Log Entry
        //        AdminLog logEntry = new AdminLog();

        //        //reference info
        //        logEntry.RefUserName = transfer.UserName;
        //        logEntry.RefSubverse = transfer.Subverse;
        //        logEntry.RefUrl = transfer.TransferRequestUrl;
        //        logEntry.RefSubmissionID = transfer.SubmissionID;

        //        logEntry.Type = "SubverseTransfer";
        //        logEntry.Action = (transfer.IsApproved ? "Approved" : "Denied");
        //        logEntry.InternalDetails = transfer.Reason;
        //        logEntry.Details = String.Format("Request to transfer subverse {0} to {1} has been {2}", transfer.Subverse, transfer.UserName, logEntry.Action);

        //        SaveAdminLogEntry(logEntry);



        //        //Send user transfer message
        //        if (!String.IsNullOrEmpty(transfer.MessageToRequestor)) {

        //            if (transfer.SubmissionID > 0) {
        //                PostComment(transfer.SubmissionID, null, String.Format("{0}: {1}", (transfer.IsApproved ? "Approved" : "Denied"), transfer.MessageToRequestor));
        //            } else {
        //                string title = (String.IsNullOrEmpty(transfer.Subverse) ? "Subverse Transfer" : String.Format("/v/{0} Transfer", transfer.Subverse));
        //                SendMessage(new ApiSendUserMessage() { Message = transfer.MessageToRequestor, Recipient = transfer.UserName, Subject = title });
        //            }
        //        }

        //    }
        //}


        #endregion

        #region Block

        /// <summary>
        /// Unblocks a domain type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        public void Unblock(DomainType domainType, string name)
        {
            Block(domainType, name, false);
        }

        /// <summary>
        /// Blocks a domain type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        public void Block(DomainType domainType, string name)
        {
            Block(domainType, name, true);
        }
        /// <summary>
        /// Blocks, Unblocks, or Toggles blocks
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        /// <param name="block">If null then toggles, else, blocks or unblocks based on value</param>
        public void Block(DomainType domainType, string name, bool? block)
        {
            DemandAuthentication();

            using (var db = new voatEntities())
            {
                switch (domainType)
                {
                    case DomainType.Subverse:

                        var exists = db.Subverses.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                        {
                            throw new VoatNotFoundException("Subverse '{0}' does not exist", name);
                        }

                        var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.Subverse.ToLower() == name.ToLower() && n.UserName == User.Identity.Name);
                        if (subverseBlock == null && ((block.HasValue && block.Value) || !block.HasValue))
                        {
                            db.UserBlockedSubverses.Add(new UserBlockedSubverse { UserName = User.Identity.Name, Subverse = name, CreationDate = Repository.CurrentDate });
                        }
                        else if (subverseBlock != null && ((block.HasValue && !block.Value) || !block.HasValue))
                        {
                            db.UserBlockedSubverses.Remove(subverseBlock);
                        }
                        db.SaveChanges();
                        break;
                    case DomainType.User:

                        var userBlock = db.UserBlockedUsers.FirstOrDefault(n => n.BlockUser.ToLower() == name.ToLower() && n.UserName == User.Identity.Name);
                        if (userBlock == null && ((block.HasValue && block.Value) || !block.HasValue))
                        {
                            db.UserBlockedUsers.Add(new UserBlockedUser { UserName = User.Identity.Name, BlockUser = name, CreationDate = Repository.CurrentDate });
                        }
                        else if (userBlock != null && ((block.HasValue && !block.Value) || !block.HasValue))
                        {
                            db.UserBlockedUsers.Remove(userBlock);
                        }

                        db.SaveChanges();
                        break;

                    default:
                        throw new NotImplementedException(String.Format("Blocking of {0} is not implemented yet", domainType.ToString()));
                        break;

                }
            }
        }

        #endregion

        #region Misc

        public bool HasAddressVoted(string addressHash, ContentType contentType, int id)
        {
            var result = true;
            switch (contentType)
            {
                case ContentType.Comment:
                    result = _db.CommentVoteTrackers.Any(x => x.CommentID == id && x.IPAddress == addressHash);
                    break;
                case ContentType.Submission:
                    result = _db.SubmissionVoteTrackers.Any(x => x.SubmissionID == id && x.IPAddress == addressHash);
                    break;
            }
            return result;
        }
        private static IQueryable<Models.Submission> ApplySubmissionSearch(SearchOptions options, IQueryable<Models.Submission> query)
        {
            //HACK: Warning, Super hacktastic
            if (!String.IsNullOrEmpty(options.Phrase))
            {

                //WARNING: This is a quickie that views spaces as AND conditions in a search.
                List<string> keywords = null;
                if (options.Phrase.Contains(" "))
                {
                    keywords = new List<string>(options.Phrase.Split(' '));
                }
                else
                {
                    keywords = new List<string>(new string[] { options.Phrase });
                }

                keywords.ForEach(x => {
                    query = query.Where(m => m.Title.Contains(x) || m.Content.Contains(x) || m.LinkDescription.Contains(x));
                });

            }

            if (options.StartDate.HasValue)
            {
                query = query.Where(x => x.CreationDate >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue)
            {
                query = query.Where(x => x.CreationDate <= options.EndDate.Value);
            }
            //Search Options
            switch (options.Sort)
            {

                case SortAlgorithm.RelativeRank:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.RelativeRank);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.RelativeRank);
                    }
                    break;
                case SortAlgorithm.Rank:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.Rank);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Rank);
                    }
                    break;

                case SortAlgorithm.New:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.CreationDate);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.CreationDate);
                    }
                    break;

                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.UpCount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.UpCount);
                    }
                    break;
                case SortAlgorithm.Viewed:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.Views);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Views);
                    }
                    break;
                case SortAlgorithm.Discussed:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.Comments.Count);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Comments.Count);
                    }
                    break;
                case SortAlgorithm.Active:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                    }
                    break;
                case SortAlgorithm.Bottom:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.DownCount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;
                case SortAlgorithm.Intensity:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.UpCount + x.DownCount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.UpCount + x.DownCount);
                    }
                    break;
            }

            query = query.Skip(options.Index).Take(options.Count);
            return query;
        }

        private static IQueryable<Models.Comment> ApplyCommentSearch(SearchOptions options, IQueryable<Models.Comment> query)
        {

            if (!String.IsNullOrEmpty(options.Phrase))
            {
                //TODO: This is a hack that views Spaces as AND conditions in a search.
                List<string> keywords = null;
                if (!String.IsNullOrEmpty(options.Phrase) && options.Phrase.Contains(" "))
                {
                    keywords = new List<string>(options.Phrase.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    keywords = new List<string>(new string[] { options.Phrase });
                }

                keywords.ForEach(x => {
                    query = query.Where(m => m.Content.Contains(x));
                });

            }
            if (options.StartDate.HasValue)
            {
                query = query.Where(x => x.CreationDate >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue)
            {
                query = query.Where(x => x.CreationDate <= options.EndDate.Value);
            }
            //TODO: Implement Depth in Comment Table
            //if (options.Depth > 0) {
            //    query = query.Where(x => 1 == 1);
            //}

            switch (options.Sort)
            {
                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.UpCount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;
                case SortAlgorithm.New:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.CreationDate);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.CreationDate);
                    }
                    break;
                case SortAlgorithm.Bottom:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => x.DownCount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;
                default:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query = query.OrderBy(x => (x.UpCount - x.DownCount));
                    }
                    else
                    {
                        query = query.OrderByDescending(x => (x.UpCount - x.DownCount));
                    }
                    break;
            }

            query = query.Skip(options.Index).Take(options.Count);


            return query;
        }

        public IEnumerable<BannedDomain> GetBannedDomains()
        {
            return (from x in _db.BannedDomains
                    orderby x.CreationDate descending
                    select x).ToList();
        }

        public bool IsUserModerator(string subverse, string userName = null, int? power = null)
        {

            DemandAuthentication();
            userName = (userName == null ? User.Identity.Name : userName);

            //if (UserHelper.IsUserSubverseModerator(User.Identity.Name, commentSubverse))
            var m = new QuerySubverseModerators(subverse);
            var mods = Task.Run(() => m.ExecuteAsync()).Result;
            return mods.Any(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && (power == null ? true : (x.Power <= power)));
        }

        public bool IsSystemSubverse(string subverse)
        {
            return IsUserModerator(subverse, "System", 1);
        }

        public string SubverseForComment(int commentID)
        {
            var subname = (from x in _db.Comments
                           where x.ID == commentID
                           select x.Submission.Subverse).FirstOrDefault();
            return subname;
        }
        private IPrincipal User
        {
            get
            {
                return System.Threading.Thread.CurrentPrincipal;
            }
        }

        public bool SubverseExists(string subverse)
        {
            return _db.Subverses.Any(x => x.Name == subverse);
        }

        public string ToCorrectSubverseCasing(string subverse)
        {
            if (!String.IsNullOrEmpty(subverse))
            {
                var sub = _db.Subverses.FirstOrDefault(x => x.Name == subverse);
                return (sub == null ? null : sub.Name);
            }
            else
            {
                return null;
            }
        }

        public string ToCorrectUserNameCasing(string userName)
        {
            if (!String.IsNullOrEmpty(userName))
            {
                return UserHelper.OriginalUsername(userName);
            }
            else
            {
                return null;
            }
        }

        private void DemandAuthentication()
        {
            if (!User.Identity.IsAuthenticated || String.IsNullOrEmpty(User.Identity.Name))
            {
                throw new VoatSecurityException("Current process not authenticated.");
            }
        }

        //TODO: Make async
        public Models.EventLog Log(EventLog log)
        {
            var newLog = _db.EventLogs.Add(log);
            _db.SaveChanges();
            return newLog;
        }

        public static DateTime CurrentDate
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            Dispose(false);
        }

        ~Repository()
        {
            Dispose(true);
        }

        protected void Dispose(bool gcCalling)
        {
            if (_db != null)
            {
                _db.Dispose();
            }
            if (!gcCalling)
            {
                System.GC.SuppressFinalize(this);
            }
        }

        protected CommandResponse<T> MapRuleOutCome<T>(RuleOutcome outcome, T result)
        {
            switch (outcome.Result)
            {
                case RuleResult.Denied:
                    return CommandResponse.Denied<T>(result, outcome.Message);
                default:
                    return CommandResponse.Success(result);
            }
        }

        #endregion

    }
}