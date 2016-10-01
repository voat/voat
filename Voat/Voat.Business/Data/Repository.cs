﻿using System;
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
using Voat.Rules.Voting;
using Voat.Domain;

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
            
            var synclock_comment = _lockStore.GetLockObject(String.Format("comment:{0}", commentID));
            lock (synclock_comment)
            {
                var comment = _db.Comments.FirstOrDefault(x => x.ID == commentID);

                if (comment != null)
                {

                    if (comment.IsDeleted)
                    {
                        return VoteResponse.Ignored(0, "Deleted comments cannot be voted");
                        //throw new VoatValidationException("Deleted comments cannot be voted");
                    }

                    //ignore votes if user owns it
                    if (String.Equals(comment.UserName, userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    //check existing vote
                    int existingVote = 0;
                    var existingVoteTracker = _db.CommentVoteTrackers.FirstOrDefault(x => x.CommentID == commentID && x.UserName == userName);
                    if (existingVoteTracker != null && existingVoteTracker.VoteStatus.HasValue)
                    {
                        existingVote = existingVoteTracker.VoteStatus.Value;
                    }

                    // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                    if (existingVote == vote && !revokeOnRevote)
                    {
                        return VoteResponse.Ignored(existingVote, "User has already voted this way.");
                    }

                    //set properties for rules engine
                    ruleContext.CommentID = commentID;
                    ruleContext.SubmissionID = comment.SubmissionID;
                    ruleContext.PropertyBag.CurrentVoteValue = existingVote; //set existing vote value so rules engine can avoid checks on revotes
                                        
                    //execute rules engine 
                    switch (vote)
                    {
                        case 1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteComment, RuleScope.UpVote, RuleScope.UpVoteComment);
                            break;
                        case -1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteComment, RuleScope.DownVote, RuleScope.DownVoteComment);
                            break;
                    }

                    //return if rules engine denies
                    if (outcome.IsDenied)
                    {
                        return VoteResponse.Create(outcome);
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
                                    response = VoteResponse.Successful(vote);
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

                                        response = VoteResponse.Successful(0, REVOKE_MSG);
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

                                        response = VoteResponse.Successful(vote);
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
                                        response = VoteResponse.Successful(0, REVOKE_MSG);
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
                                        response = VoteResponse.Successful(vote);
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
                    if (submission.IsDeleted)
                    {
                        return VoteResponse.Ignored(0, "Deleted submissions cannot be voted");
                        //return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    //ignore votes if user owns it
                    if (String.Equals(submission.UserName, userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    //check existing vote
                    int existingVote = 0;
                    var existingVoteTracker = _db.SubmissionVoteTrackers.FirstOrDefault(x => x.SubmissionID == submissionID && x.UserName == userName);
                    if (existingVoteTracker != null && existingVoteTracker.VoteStatus.HasValue)
                    {
                        existingVote = existingVoteTracker.VoteStatus.Value;
                    }

                    // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                    if (existingVote == vote && !revokeOnRevote)
                    {
                        return VoteResponse.Ignored(existingVote, "User has already voted this way.");
                    }

                    //set properties for rules engine
                    ruleContext.SubmissionID = submission.ID;
                    ruleContext.PropertyBag.CurrentVoteValue = existingVote; //set existing vote value so rules engine can avoid checks on revotes

                    //execute rules engine 
                    switch (vote)
                    {
                        case 1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteSubmission, RuleScope.UpVote, RuleScope.UpVoteSubmission);
                            break;
                        case -1:
                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(ruleContext, RuleScope.Vote, RuleScope.VoteSubmission, RuleScope.DownVote, RuleScope.DownVoteSubmission);
                            break;
                    }

                    //return if rules engine denies
                    if (outcome.IsDenied)
                    {
                        return VoteResponse.Create(outcome);
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
                                    
                                    //calculate new ranks
                                    Ranking.RerankSubmission(submission);

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

                                    response = VoteResponse.Successful(vote);
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
                                        
                                        //calculate new ranks
                                        Ranking.RerankSubmission(submission);

                                        _db.SubmissionVoteTrackers.Remove(existingVoteTracker);
                                        _db.SaveChanges();

                                        response = response = VoteResponse.Successful(0, REVOKE_MSG);
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
                                        
                                        //calculate new ranks
                                        Ranking.RerankSubmission(submission);

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;

                                        _db.SaveChanges();

                                        response = VoteResponse.Successful(vote);
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

                                        //calculate new ranks
                                        Ranking.RerankSubmission(submission);

                                        _db.SubmissionVoteTrackers.Remove(existingVoteTracker);
                                        _db.SaveChanges();

                                        response = VoteResponse.Successful(0, REVOKE_MSG);
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

                                        //calculate new ranks
                                        Ranking.RerankSubmission(submission);

                                        existingVoteTracker.VoteStatus = vote;
                                        existingVoteTracker.CreationDate = CurrentDate;

                                        _db.SaveChanges();
                                        response = VoteResponse.Successful(vote);
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

        public Subverse GetSubverseInfo(string subverse, bool filterDisabled = false)
        {
            var query = (from x in _db.Subverses
                         where x.Name == subverse
                         select x);
            if (filterDisabled)
            {
                query = query.Where(x => x.IsAdminDisabled != true);
            }
            var submission = query.FirstOrDefault();
            return submission;
        }
        public string GetSubverseStylesheet(string subverse)
        {

            var sheet = (from x in _db.Subverses
                         where x.Name.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                         select x.Stylesheet).FirstOrDefault();
            return String.IsNullOrEmpty(sheet) ? "" : sheet;
        }
        public IEnumerable<SubverseBan> GetSubverseUserBans(string subverse)
        {
            var data = (from x in _db.SubverseBans
                        where x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        orderby x.CreationDate ascending
                        select x).ToList();

            return data.AsEnumerable();
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
            var record = Selectors.SecureSubmission(GetSubmissionUnprotected(submissionID));
            return record;
        }
        public string GetSubmissionOwnerName(int submissionID)
        {
            var result = (from x in _db.Submissions
                          where x.ID == submissionID
                          select x.UserName).FirstOrDefault();
            return result;
        }

        public Models.Submission FindSubverseLinkSubmission(string subverse, string url, TimeSpan cutOffTimeSpan)
        {
            var cutOffDate = CurrentDate.Subtract(cutOffTimeSpan);
            return _db.Submissions.AsNoTracking().FirstOrDefault(s => 
                s.Content.Equals(url, StringComparison.OrdinalIgnoreCase) 
                && s.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                && s.CreationDate > cutOffDate);
        }
        public int FindUserLinkSubmissionCount(string userName, string url, TimeSpan cutOffTimeSpan)
        {
            var cutOffDate = CurrentDate.Subtract(cutOffTimeSpan);
            return _db.Submissions.Count(s =>
                s.Content.Equals(url, StringComparison.OrdinalIgnoreCase)
                && s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                && s.CreationDate > cutOffDate);
        }
        private Models.Submission GetSubmissionUnprotected(int submissionID)
        {

            var query = (from x in _db.Submissions.AsNoTracking()
                         where x.ID == submissionID
                         select x);

            var record = query.FirstOrDefault();
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
                             where 
                             !x.Subverse1.IsAdminPrivate 
                             && !x.Subverse1.IsPrivate
                             && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value)
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
                                    && (!x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value))
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

            if (User.Identity.IsAuthenticated)
            {
                //filter blocked subs
                query = query.Where(s => !_db.UserBlockedSubverses.Where(b => 
                    b.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) 
                    && b.Subverse.Equals(s.Subverse, StringComparison.OrdinalIgnoreCase)).Any());
                //filter blocked users (Currently commented out do to a collation issue)
                query = query.Where(s => !_db.UserBlockedUsers.Where(b =>
                    b.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                    && s.UserName.Equals(b.BlockUser, StringComparison.OrdinalIgnoreCase)
                    ).Any());
                //filter global banned users
                query = query.Where(s => !_db.BannedUsers.Where(b => b.UserName.Equals(s.UserName, StringComparison.OrdinalIgnoreCase)).Any());
            }

            query = ApplySubmissionSearch(options, query);
            
            //execute query
            var results = query.Select(Selectors.SecureSubmission).ToList();

            return results.AsEnumerable();
        }

        [Authorize]
        public async Task<CommandResponse<Models.Submission>> PostSubmission(UserSubmission userSubmission)
        {
            DemandAuthentication();
            //Load Subverse Object
            var cmdSubverse = new QuerySubverse(userSubmission.Subverse);
            var subverseObject = cmdSubverse.Execute();

            //Evaluate Rules
            var context = new VoatRuleContext();
            context.Subverse = subverseObject;
            context.PropertyBag.UserSubmission = userSubmission;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.Post, RuleScope.PostSubmission);
            //if rules engine denies bail.
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Models.Submission>(outcome, null);
            }

            //Save submission
            Models.Submission m = new Models.Submission();
            m.UpCount = 1; //https://voat.co/v/PreviewAPI/comments/877596
            m.UserName = User.Identity.Name;
            m.CreationDate = CurrentDate;
            m.Subverse = subverseObject.Name;
            //TODO: Allow this value to be passed in and verified instead of hard coding
            m.IsAnonymized = subverseObject.IsAnonymized;
            //m.IsAdult = submission.IsAdult;

            //1: Text, 2: Link
            m.Type = (int)userSubmission.Type;

            if (userSubmission.Type == SubmissionType.Text)
            {

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                {
                    userSubmission.Content = ContentProcessor.Instance.Process(userSubmission.Content, ProcessingStage.InboundPreSave, m);
                }

                m.Title = userSubmission.Title;
                m.Content = userSubmission.Content;
                m.FormattedContent = Formatting.FormatMessage(userSubmission.Content, true);

            }
            else
            {
                m.Title = userSubmission.Title;
                m.Url = userSubmission.Url;

                if (subverseObject.IsThumbnailEnabled)
                {
                    // try to generate and assign a thumbnail to submission model
                    m.Thumbnail = await ThumbGenerator.GenerateThumbFromWebpageUrl(userSubmission.Url);
                }
            }

            _db.Submissions.Add(m);

            await _db.SaveChangesAsync();

            //This sends notifications by parsing content
            if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            {
                ContentProcessor.Instance.Process(m.Content, ProcessingStage.InboundPostSave, m);
            }

            return CommandResponse.Successful(Selectors.SecureSubmission(m));
        }

        [Authorize]
        public Models.Submission EditSubmission(int submissionID, UserSubmission userSubmission)
        {
            if (userSubmission == null || (!userSubmission.HasState && String.IsNullOrEmpty(userSubmission.Content)))
            {
                throw new VoatValidationException("The submission must not be null or have invalid state");
            }

            //if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content)) {
            //    throw new VoatValidationException("Either a Url or Content must be provided.");
            //}

            var submission = _db.Submissions.Where(x => x.ID == submissionID).FirstOrDefault();

            if (submission == null)
            {
                throw new VoatNotFoundException(String.Format("Can't find submission with ID {0}", submissionID));
            }

            if (submission.IsDeleted)
            {
                throw new VoatValidationException("Deleted submissions cannot be edited");
            }


            if (submission.UserName != User.Identity.Name)
            {
                throw new VoatSecurityException(String.Format("Submission can not be edited by account"));
            }

            //only allow edits for self posts
            if (submission.Type == 1)
            {
                submission.Content = userSubmission.Content ?? submission.Content;
                submission.FormattedContent = Formatting.FormatMessage(submission.Content, true);
            }

            //allow edit of title if in 10 minute window
            if (CurrentDate.Subtract(submission.CreationDate).TotalMinutes <= 10.0f)
            {

                if (!String.IsNullOrEmpty(userSubmission.Title) && Formatting.ContainsUnicode(userSubmission.Title))
                {
                    throw new VoatValidationException("Submission title can not contain Unicode characters");
                }

                submission.Title = (String.IsNullOrEmpty(userSubmission.Title) ? submission.Title : userSubmission.Title);
            }

            submission.LastEditDate = CurrentDate;

            _db.SaveChanges();

            return Selectors.SecureSubmission(submission);
        }

        [Authorize]
        //LOGIC COPIED FROM SubmissionController.DeleteSubmission(int)
        public Models.Submission DeleteSubmission(int submissionID, string reason = null)
        {
            DemandAuthentication();

            var submission = _db.Submissions.Find(submissionID);

            if (submission != null && !submission.IsDeleted)
            {
                // delete submission if delete request is issued by submission author
                if (submission.UserName == User.Identity.Name)
                {
                    submission.IsDeleted = true;

                    if (submission.Type == 1)
                    {
                        submission.Content = "Deleted by author at " + Repository.CurrentDate;
                    }
                    else
                    {
                        submission.Content = "http://voat.co";
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
                else if (IsUserModerator(submission.Subverse))
                {

                    if (String.IsNullOrEmpty(reason))
                    {
                        var ex = new VoatValidationException("A reason for deletion is required");
                        ex.Data["SubmissionID"] = submissionID;
                        throw ex;
                    }

                    // mark submission as deleted
                    submission.IsDeleted = true;

                    // move the submission to removal log
                    var removalLog = new SubmissionRemovalLog
                    {
                        SubmissionID = submission.ID,
                        Moderator = User.Identity.Name,
                        Reason = "This feature is not yet implemented",
                        CreationDate = Repository.CurrentDate
                    };

                    _db.SubmissionRemovalLogs.Add(removalLog);

                    // notify submission author that his submission has been deleted by a moderator
                    var message = new Domain.Models.SendMessage()
                    {
                        Sender = $"v/{submission.Subverse}",
                        Recipient = submission.UserName,
                        Subject = "Your submission has been deleted by a moderator",
                        Message = "Your [submission](/v/" + submission.Subverse + "/comments/" + submission.ID + ") has been deleted by: " +
                                    "/u/" + User.Identity.Name + " at " + Repository.CurrentDate + "  " + Environment.NewLine +
                                    "Original submission content was: " + Environment.NewLine +
                                    "---" + Environment.NewLine +
                                    (submission.Type == 1 ?
                                    "Submission title: " + submission.Title + ", " + Environment.NewLine +
                                    "Submission content: " + submission.Content
                                    :
                                    "Link description: " + submission.Title + ", " + Environment.NewLine +
                                    "Link URL: " + submission.Url
                                    )
                    };
                    var cmd = new SendMessageCommand(message);
                    cmd.Execute();

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionID);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmissions.Remove(existingSticky);
                    }

                    _db.SaveChanges();

                }
                else
                {
                    throw new VoatSecurityException("User doesn't have permission to delete submission.");
                }
            }

            return Selectors.SecureSubmission(submission);
        }

        #endregion

        #region Comments 

        public IEnumerable<Domain.Models.Comment> GetUserComments(string userName, SearchOptions options)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new VoatValidationException("A user name must be provided.");
            }
            if (!UserHelper.UserExists(userName))
            {
                throw new VoatValidationException("User '{0}' does not exist.", userName);
            }

            var query = (from comment in _db.Comments
                         join submission in _db.Submissions on comment.SubmissionID equals submission.ID
                         where
                            !comment.IsAnonymized 
                            && !comment.IsDeleted
                            && (comment.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                         select new Domain.Models.Comment()
                         {
                             ID = comment.ID,
                             ParentID = comment.ParentID,
                             Content = comment.Content,
                             FormattedContent = comment.FormattedContent,
                             UserName = comment.UserName,
                             UpCount = (int)comment.UpCount,
                             DownCount = (int)comment.DownCount,
                             CreationDate = comment.CreationDate,
                             IsAnonymized = comment.IsAnonymized,
                             IsDeleted = comment.IsDeleted,
                             IsDistinguished = comment.IsDistinguished,
                             LastEditDate = comment.LastEditDate,
                             SubmissionID = comment.SubmissionID,
                             Subverse = submission.Subverse
                         });


            query = ApplyCommentSearch(options, query);
            var results = query.ToList();

            return results;
        }
        public IEnumerable<Domain.Models.Comment> GetComments(string subverse, SearchOptions options)
        {
            var query = (from comment in _db.Comments
                         join submission in _db.Submissions on comment.SubmissionID equals submission.ID
                         where 
                         !comment.IsDeleted
                         && (submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || String.IsNullOrEmpty(subverse))
                         select new Domain.Models.Comment()
                         {
                             ID = comment.ID,
                             ParentID = comment.ParentID,
                             Content = comment.Content,
                             FormattedContent = comment.FormattedContent,
                             UserName = comment.UserName,
                             UpCount = (int)comment.UpCount,
                             DownCount = (int)comment.DownCount,
                             CreationDate = comment.CreationDate,
                             IsAnonymized = comment.IsAnonymized,
                             IsDeleted = comment.IsDeleted,
                             IsDistinguished = comment.IsDistinguished,
                             LastEditDate = comment.LastEditDate,
                             SubmissionID = comment.SubmissionID,
                             Subverse = submission.Subverse
                         });

            query = ApplyCommentSearch(options, query);
            var results = query.ToList();

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
            var results = commentTree.ToList();
            return results;
        }

        public Domain.Models.Comment GetComment(int commentID)
        {
            var query = (from comment in _db.Comments
                         join submission in _db.Submissions on comment.SubmissionID equals submission.ID
                         where
                         comment.ID == commentID
                         select new Domain.Models.Comment()
                         {
                             ID = comment.ID,
                             ParentID = comment.ParentID,
                             Content = comment.Content,
                             FormattedContent = comment.FormattedContent,
                             UserName = comment.UserName,
                             UpCount = (int)comment.UpCount,
                             DownCount = (int)comment.DownCount,
                             CreationDate = comment.CreationDate,
                             IsAnonymized = comment.IsAnonymized,
                             IsDeleted = comment.IsDeleted,
                             IsDistinguished = comment.IsDistinguished,
                             LastEditDate = comment.LastEditDate,
                             SubmissionID = comment.SubmissionID,
                             Subverse = submission.Subverse
                         });

            var record = query.FirstOrDefault();
           
            DomainMaps.ProcessComment(record, true);

            return record;
        }
        
        public async Task<CommandResponse<Data.Models.Comment>> DeleteComment(int commentID, string reason = null)
        {

            DemandAuthentication();

            var comment = _db.Comments.Find(commentID);

            if (comment != null && !comment.IsDeleted)
            {
                var submission = _db.Submissions.Find(comment.SubmissionID);
                if (submission != null)
                {
                    var subverseName = submission.Subverse;
                    // delete comment if the comment author is currently logged in user
                    if (comment.UserName == User.Identity.Name)
                    {
                        comment.IsDeleted = true;
                        comment.Content = "Deleted by author at " + Repository.CurrentDate;
                        await _db.SaveChangesAsync();
                    }

                    // delete comment if delete request is issued by subverse moderator
                    else
                    {
                        if (IsUserModerator(subverseName))
                        {
                            if (String.IsNullOrEmpty(reason))
                            {
                                var ex = new VoatValidationException("A reason for deletion is required");
                                ex.Data["CommentID"] = commentID;
                                throw ex;
                            }

                            // notify comment author that his comment has been deleted by a moderator
                            var message = new Domain.Models.SendMessage()
                            {
                                Sender = $"v/{subverseName}",
                                Recipient = comment.UserName,
                                Subject = "Your comment has been deleted by a moderator",
                                Message = "Your [comment](/v/" + subverseName + "/comments/" + comment.SubmissionID + "/" + comment.ID + ") has been deleted by: " +
                                            "/u/" + User.Identity.Name + " on: " + Repository.CurrentDate + "  " + Environment.NewLine +
                                            "Original comment content was: " + Environment.NewLine +
                                            "---" + Environment.NewLine +
                                            comment.Content
                            };
                            var cmd = new SendMessageCommand(message);
                            await cmd.Execute();

                            comment.IsDeleted = true;

                            // move the comment to removal log
                            var removalLog = new CommentRemovalLog
                            {
                                CommentID = comment.ID,
                                Moderator = User.Identity.Name,
                                Reason = reason,
                                CreationDate = Repository.CurrentDate
                            };

                            _db.CommentRemovalLogs.Add(removalLog);

                            comment.Content = "Deleted by a moderator at " + Repository.CurrentDate;
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                            ex.Data["CommentID"] = commentID;
                            throw ex;
                        }
                    }
                }
            }
            return CommandResponse.Successful(Selectors.SecureComment(comment));
        }

        [Authorize]
        public async Task<CommandResponse<Data.Models.Comment>> EditComment(int commentID, string content)
        {
            DemandAuthentication();

            var comment = _db.Comments.Find(commentID);

            if (comment != null)
            {
                if (comment.UserName != User.Identity.Name)
                {
                    return CommandResponse.Denied<Data.Models.Comment>(null, "User doesn't have permissions to perform requested action");
                }

                //evaluate rule
                VoatRuleContext context = new VoatRuleContext();
                //set any state we have so context doesn't have to retrieve
                context.SubmissionID = comment.SubmissionID;
                context.PropertyBag.CommentContent = content;
                context.PropertyBag.Comment = comment;

                var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.EditComment);

                if (outcome.IsAllowed)
                {
                    comment.LastEditDate = CurrentDate;
                    comment.Content = content;

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                    {
                        comment.Content = ContentProcessor.Instance.Process(comment.Content, ProcessingStage.InboundPreSave, comment);
                    }

                    var formattedComment = Voat.Utilities.Formatting.FormatMessage(comment.Content);
                    comment.FormattedContent = formattedComment;

                    await _db.SaveChangesAsync();
                }
                else
                {
                    return MapRuleOutCome<Data.Models.Comment>(outcome, null);
                }
            }
            else {
                throw new VoatNotFoundException("Can not find comment with ID {0}", commentID);
            }

            return CommandResponse.Successful(Selectors.SecureComment(comment));
        }

        public async Task<CommandResponse<Domain.Models.Comment>> PostCommentReply(int parentCommentID, string comment)
        {
            var c = _db.Comments.Find(parentCommentID);
            if (c == null)
            {
                throw new VoatNotFoundException("Can not find parent comment with id {0}", parentCommentID.ToString());
            }
            var submissionid = c.SubmissionID;
            return await PostComment(submissionid.Value, parentCommentID, comment);
        }

        public async Task<CommandResponse<Domain.Models.Comment>> PostComment(int submissionID, int? parentCommentID, string commentContent)
        {

            DemandAuthentication();

            var submission = GetSubmission(submissionID);
            if (submission == null)
            {
                throw new VoatNotFoundException("submissionID", submissionID, "Can not find submission");
            }

            //evaluate rule
            VoatRuleContext context = new VoatRuleContext();
            //set any state we have so context doesn't have to retrieve
            context.SubmissionID = submissionID;
            context.PropertyBag.CommentContent = commentContent;
            
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.PostComment);

            if (outcome.IsAllowed)
            {
                //Save comment
                var c = new Models.Comment();
                c.CreationDate = Repository.CurrentDate;
                c.UserName = User.Identity.Name;
                c.ParentID = (parentCommentID > 0 ? parentCommentID : (int?)null);
                c.SubmissionID = submissionID;
                c.Votes = 0;
                c.UpCount = 0;
                //TODO: Ensure this is acceptable
                //c.IsAnonymized = (submission.IsAnonymized || subverse.IsAnonymized);
                c.IsAnonymized = submission.IsAnonymized;
                
                c.Content = ContentProcessor.Instance.Process(commentContent, ProcessingStage.InboundPreSave, c);
                //save fully formatted content 
                var formattedComment = Formatting.FormatMessage(c.Content);
                c.FormattedContent = formattedComment;

                _db.Comments.Add(c);
                await _db.SaveChangesAsync();

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                {
                    ContentProcessor.Instance.Process(c.Content, ProcessingStage.InboundPostSave, c);
                }

                await NotificationManager.SendCommentNotification(c);

                return MapRuleOutCome(outcome, DomainMaps.Map(Selectors.SecureComment(c), submission.Subverse));
            }

            return MapRuleOutCome(outcome, (Domain.Models.Comment)null);
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
        public static void SetDefaultUserPreferences(Data.Models.UserPreference p)
        {
            p.Language = "en";
            p.NightMode = false;
            p.OpenInNewWindow = false;
            p.UseSubscriptionsMenu = true;
            p.DisableCSS = false;
            p.DisplaySubscriptions = false;
            p.DisplayVotes = false;
            p.EnableAdultContent = false;
            p.Bio = "Aww snap, this user did not yet write their bio. If they did, it would show up here, you know.";
            p.Avatar = "default.jpg";
            p.CollapseCommentLimit = 4;
            p.DisplayCommentCount = 5;
            p.HighlightMinutes = 30;
            p.VanityTitle = null;
        }
        [Authorize]
        public void SaveUserPrefernces(Domain.Models.UserPreference preferences)
        {
            DemandAuthentication();

            var p = (from x in _db.UserPreferences
                     where x.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();

            if (p == null)
            {
                p = new Data.Models.UserPreference();
                p.UserName = User.Identity.Name;
                SetDefaultUserPreferences(p);
                _db.UserPreferences.Add(p);
            }

            //if (!String.IsNullOrEmpty(preferences.Avatar))
            //{
            //    p.Avatar = preferences.Avatar;
            //}
            if (!String.IsNullOrEmpty(preferences.Bio))
            {
                p.Bio = preferences.Bio;
            }
            if (!String.IsNullOrEmpty(preferences.Language))
            {
                p.Language = preferences.Language;
            }
            if (preferences.OpenInNewWindow.HasValue)
            {
                p.OpenInNewWindow = preferences.OpenInNewWindow.Value;
            }
            if (preferences.DisableCSS.HasValue)
            {
                p.DisableCSS = preferences.DisableCSS.Value;
            }
            if (preferences.EnableAdultContent.HasValue)
            {
                p.EnableAdultContent = preferences.EnableAdultContent.Value;
            }
            if (preferences.NightMode.HasValue)
            {
                p.NightMode = preferences.NightMode.Value;
            }
            if (preferences.DisplaySubscriptions.HasValue)
            {
                p.DisplaySubscriptions = preferences.DisplaySubscriptions.Value;
            }
            if (preferences.DisplayVotes.HasValue)
            {
                p.DisplayVotes = preferences.DisplayVotes.Value;
            }
            if (preferences.UseSubscriptionsMenu.HasValue)
            {
                p.UseSubscriptionsMenu = preferences.UseSubscriptionsMenu.Value;
            }
            if (preferences.DisplayCommentCount.HasValue)
            {
                p.DisplayCommentCount = preferences.DisplayCommentCount.Value;
            }
            if (preferences.HighlightMinutes.HasValue)
            {
                p.HighlightMinutes = preferences.HighlightMinutes.Value;
            }
            if (!String.IsNullOrEmpty(preferences.VanityTitle))
            {
                p.VanityTitle = preferences.VanityTitle;
            }
            if (preferences.CollapseCommentLimit.HasValue)
            {
                p.CollapseCommentLimit = preferences.CollapseCommentLimit.Value;
            }
            if (preferences.DisplayAds.HasValue)
            {
                p.DisplayAds = preferences.DisplayAds.Value;
            }
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
                return new CommandResponse(Status.Success, "Submission reply sent");
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

            //If sender isn't a subverse (automated messages) run sender checks
            if (!Regex.IsMatch(message.Sender, @"^v/\w*"))
            {
                if (Voat.Utilities.BanningUtility.ContentContainsBannedDomain(null, message.Message))
                {
                    return CommandResponse.Ignored("Message contains banned domain");
                }
                if (Voat.Utilities.UserHelper.IsUserGloballyBanned(message.Sender))
                {
                    return CommandResponse.Ignored("User is banned");
                }
                if (Voat.Utilities.Karma.CommentKarma(message.Sender) < 10)
                {
                    return CommandResponse.Ignored("Comment points too low to send messages", "CCP < 10");
                }
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

                if (!String.IsNullOrEmpty(prefix) && prefix.ToLower().Contains("v"))
                {
                    //don't allow banned users to send to subverses
                    if (!UserHelper.IsUserBannedFromSubverse(message.Sender, recipient))
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
                                    Sender = message.Sender,
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
                    //ensure proper cased
                    recipient = UserHelper.OriginalUsername(recipient);

                    if (!String.IsNullOrEmpty(recipient) && Voat.Utilities.UserHelper.UserExists(recipient))
                    {
                        messages.Add(new PrivateMessage
                        {
                            Sender = message.Sender,
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
                        //substring - db column is nvarchar(50) and any longer breaks EF
                        messages.ForEach(x =>
                        {
                            if (x.Subject.Length > 50)
                            {
                                x.Subject = x.Subject.Substring(0, 50);
                            }
                        });

                        db.PrivateMessages.AddRange(messages);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        //TODO Log this
                        return CommandResponse.Error<CommandResponse>(ex);
                    }
                }
                //send notices async
                Task.Run(() => messages.ForEach(x => EventNotification.Instance.SendMessageNotice(x.Recipient, x.Sender, Domain.Models.MessageType.Inbox, null, null, x.Body)));
            }
            return CommandResponse.Successful();
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
                                Subject = s.Title,
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
        public IEnumerable<CommentSaveTracker> UserCommentSavedBySubmission(int submissionID, string userName)
        {
            List<CommentSaveTracker> vCache = new List<CommentSaveTracker>();

            if (!String.IsNullOrEmpty(userName))
            {
                vCache = (from cv in _db.CommentSaveTrackers.AsNoTracking()
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

        public IList<BlockedItem> GetBlockedUsers(string userName)
        {
            var blocked = (from x in _db.UserBlockedUsers
                           where x.UserName == userName
                           select new BlockedItem() { Name = x.BlockUser, Type = DomainType.User, CreationDate = x.CreationDate }).ToList();
            return blocked;
        }

        public IList<BlockedItem> GetBlockedSubverses(string userName)
        {
            var blocked = (from x in _db.UserBlockedSubverses
                           where x.UserName == userName
                           select new BlockedItem() { Name = x.Subverse, Type = DomainType.Subverse, CreationDate = x.CreationDate }).ToList();
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

            //Badges
            var userBadges = (from x in _db.UserBadges
                              join b in _db.Badges on x.BadgeID equals b.ID
                              where 
                              x.UserName == userName
                              //TODO: test this for appending alpha/beta badges to user list (aka virtual badges)
                              //||
                              //(b.ID == "alpha_user" && userInfo.RegistrationDate < (new DateTime(2015, 12, 31)))
                              //||
                              //(b.ID == "beta_user" && userInfo.RegistrationDate > (new DateTime(2015, 12, 31)))
                              select new Voat.Domain.Models.UserBadge()
                              {
                                  CreationDate = x.CreationDate,
                                  Name = b.Name,
                                  Title = b.Title,
                                  Graphic = b.Graphic,
                              }
                              ).ToList();

            userInfo.Badges = userBadges;

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
                            && 
                            ((x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.CreationDate >= compareDate)
                            && (type != null && x.Type == (int)type.Value) || type == null)
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
        public CommandResponse SubscribeUser(DomainType domainType, SubscriptionAction action, string subscriptionName)
        {
            switch (domainType)
            {
                case DomainType.Subverse:

                    var subverse = GetSubverseInfo(subscriptionName);
                    if (subverse == null)
                    {
                        return CommandResponse.Denied("Subverse does not exist");
                    }
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        return CommandResponse.Denied("Subverse is disabled");
                    }

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
            return CommandResponse.Successful();
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
        public CommandResponse<bool?> Block(DomainType domainType, string name, bool? block)
        {
            DemandAuthentication();
            var response = new CommandResponse<bool?>();
            
            using (var db = new voatEntities())
            {
                switch (domainType)
                {
                    case DomainType.Subverse:

                        var exists = db.Subverses.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (exists == null)
                        {
                            throw new VoatNotFoundException("Subverse '{0}' does not exist", name);
                        }
                        //Set propercased name
                        name = exists.Name;

                        var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.Subverse.ToLower() == name.ToLower() && n.UserName == User.Identity.Name);
                        if (subverseBlock == null && ((block.HasValue && block.Value) || !block.HasValue))
                        {
                            db.UserBlockedSubverses.Add(new UserBlockedSubverse { UserName = User.Identity.Name, Subverse = name, CreationDate = Repository.CurrentDate });
                            response.Response = true;
                        }
                        else if (subverseBlock != null && ((block.HasValue && !block.Value) || !block.HasValue))
                        {
                            db.UserBlockedSubverses.Remove(subverseBlock);
                            response.Response = false;
                        }
                        db.SaveChanges();
                        break;
                    case DomainType.User:
                        //Ensure user exists, get propercased user name
                        name = UserHelper.OriginalUsername(name);
                        if (String.IsNullOrEmpty(name))
                        {
                            return new CommandResponse<bool?>(null, Status.Error, "User does not exist");
                        }
                        var userBlock = db.UserBlockedUsers.FirstOrDefault(n => n.BlockUser.ToLower() == name.ToLower() && n.UserName == User.Identity.Name);
                        if (userBlock == null && ((block.HasValue && block.Value) || !block.HasValue))
                        {
                            db.UserBlockedUsers.Add(new UserBlockedUser { UserName = User.Identity.Name, BlockUser = name, CreationDate = Repository.CurrentDate });
                            response.Response = true;
                        }
                        else if (userBlock != null && ((block.HasValue && !block.Value) || !block.HasValue))
                        {
                            db.UserBlockedUsers.Remove(userBlock);
                            response.Response = false;
                        }

                        db.SaveChanges();
                        break;

                    default:
                        throw new NotImplementedException(String.Format("Blocking of {0} is not implemented yet", domainType.ToString()));
                        break;

                }
            }
            response.Status = Status.Success;
            return response;
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
                    query = query.Where(m => m.Title.Contains(x) || m.Content.Contains(x) || m.Url.Contains(x));
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
        private static IQueryable<Domain.Models.Comment> ApplyCommentSearch(SearchOptions options, IQueryable<Domain.Models.Comment> query)
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
                    return CommandResponse.Successful(result);
            }
        }

        #endregion

    }
}