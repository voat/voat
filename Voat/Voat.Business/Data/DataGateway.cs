//#define DISABLE_MOD

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Common;
using Voat.Data.Models;
using Voat.Models;
using Voat.Models.Api.v1;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Data {


    public class DataGateway : IDisposable {

        private voatEntities _db;

        /// <summary>
        /// PLEASE ONLY ACCESS THIS PROPERTY WHEN ABSOLUTELY NECCESSARY. 
        /// Code that uses this property can not be resused. 
        /// </summary>
        public voatEntities Entities {
            get {
                return _db;
            }
        }

        public DataGateway() {
            _db = new Models.voatEntities();
        }
        public DataGateway(Models.voatEntities dbContext) {
            _db = dbContext;
        }

        private IPrincipal User {
            get {
                return System.Threading.Thread.CurrentPrincipal;
            }
        }


        public string SubverseForComment(int commentID) {
            var subname = (from x in _db.Comments
                           where x.ID == commentID
                           select x.Submission.Subverse).FirstOrDefault();
            return subname;
        }

        public string SubverseForSubmission(int submissionID) {
            var subname = (from x in _db.Submissions
                           where x.ID == submissionID
                           select x.Subverse).FirstOrDefault();
            return subname;
        }
        
        public VoteResponse VoteSubmission(int submissionID, int vote, bool revokeOnRevote = true) {

            return Vote(submissionID, ContentType.Submission, vote, revokeOnRevote);

        }

        public VoteResponse VoteComment(int commentID, int vote, bool revokeOnRevote = true) {

            return Vote(commentID, ContentType.Comment, vote, revokeOnRevote);

        }

        [Authorize]
        private VoteResponse Vote(int id, ContentType type, int vote, bool revokeOnRevote = true) {

            //make sure we don't have bad int values for vote
            if (Math.Abs(vote) > 1) {
                throw new ArgumentOutOfRangeException("vote", "Valid values for vote are only: -1, 0, 1");
            }

            string userName = User.Identity.Name;


            //RulesEngine.Instance.Evaluate(RuleScope.UpVote, new RuleContext(userName));

            //Karma Eval
            switch (vote) {
                case 1:
                    var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.UpVote);
                    if (outcome.IsDenied) {
                        return VoteResponse.Denied(outcome.Message);
                    }
                    //if (Karma.CommentKarma(userName) <= 20 && Utils.User.TotalVotesUsedInPast24Hours(userName) >= 10) {
                    //    return VoteResponse.Denied("User has exceeded votes allowed per CCP and/or time");
                    //}
                    break;
                case -1:
                    var outcome2 = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.DownVote);
                    if (outcome2.IsDenied) {
                        return VoteResponse.Denied(outcome2.Message);
                    }
                    //int CCPThreshold = 100;
                    //if (Karma.CommentKarma(userName) < CCPThreshold) {
                    //    return VoteResponse.Denied(String.Format("Can not downvote with less than {0} CCP", CCPThreshold));
                    //}
                    
                    break;
            }

            string REVOKE_MSG = "Vote has been revoked";

            switch (type) {
                #region Comment
                case ContentType.Comment:

                    var outcome = VoatRulesEngine.Instance.IsCommentVoteAllowed(id, vote);
                    if (!outcome.IsAllowed) {
                        return new VoteResponse(ProcessResult.Denied, null, outcome.ToString());
                    }

                    var comment = _db.Comments.Include("Message").FirstOrDefault(x => x.ID == id);
                    VoatRulesEngine.Instance.Context.SubverseName = comment.Submission.Subverse;

                    if (comment != null) {

                        // do not execute voting, subverse is in anonymized mode
                        if (comment.Submission.IsAnonymized) {
                            return VoteResponse.Ignored(0, "Subverse is anonymized, voting disabled");
                        }

                        //ignore votes if comment is users
                        if (String.Equals(comment.UserName, userName, StringComparison.InvariantCultureIgnoreCase)) { 
                            return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                        }
                        
                        var existingVote = 0;
                        
                        var existingCommentVote = _db.CommentVoteTrackers.FirstOrDefault(x => x.CommentID == id && x.UserName == userName);

                        if (existingCommentVote != null && existingCommentVote.VoteStatus.HasValue) {
                            existingVote = existingCommentVote.VoteStatus.Value;
                        }

                        // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                        if (existingVote == vote && !revokeOnRevote) {
                            return VoteResponse.Ignored(existingVote, "User has already voted this way.");
                        }


                        VoteResponse response = new VoteResponse(ProcessResult.NotProcessed, 0, "Vote not processed.");
                        var votingTracker = _db.CommentVoteTrackers.FirstOrDefault(b => b.CommentID == id && b.UserName == userName);

                        switch (existingVote) {
                            

                            case 0: //Never voted or No vote

                                switch (vote) { 
                                    case 0:
                                        response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");
                                        break;
                                    case 1:
                                    case -1:
                                        
                                        if (vote == 1) {
                                            comment.UpCount++;
                                        } else {

                                            //VoatRulesEngine.Instance.Context.SubverseName = comment.Message.Subverse;

                                            //outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.DownVoteComment);
                                            //if (outcome.IsDenied) {
                                            //    return VoteResponse.Denied(outcome.ToString());
                                            //}

                                            //if (!Voat.UserHelper.CanUserDownVoteInSubverse(comment.Message.Subverses, userName)) {
                                            //    return VoteResponse.Denied("Subverse MinCPP requirement not met for downvote.");
                                            //}

                                            comment.DownCount++;
                                        }
                                
                                        var newVotingTracker = new CommentVoteTracker {
                                            CommentID = id,
                                            UserName = userName,
                                            VoteStatus = vote,
                                            CreationDate = DateTime.Now
                                        };
                                
                                        _db.CommentVoteTrackers.Add(newVotingTracker);
                                        _db.SaveChanges();

                                        //SendVoteNotification(comment.Name, "upvote");
                                        response = VoteResponse.Success(vote);
                                        break;
                                }
                                break;
                            case 1: //Previous Upvote
                                
                                switch (vote) {
                                    case 0: //revoke
                                    case 1: //revote which means revoke if we are here
                                           
                                        if (votingTracker != null) {

                                            comment.UpCount--;

                                            _db.CommentVoteTrackers.Remove(votingTracker);
                                        
                                            _db.SaveChanges();

                                            response = VoteResponse.Success(0, REVOKE_MSG);

                                        }
                                        break;
                                    case -1:
                                        //change upvote to downvote
                                        

                                        if (votingTracker != null) {

                                            comment.UpCount--;
                                            comment.DownCount++;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.CreationDate = CurrentDate;                                             
                                            _db.SaveChanges();

                                            response = VoteResponse.Success(vote);
                                        }
                                        break;
                                }

                                //SendVoteNotification(comment.Name, "downvote");
                                //ResetCommentVote(userWhichUpvoted, commentId);

                                break;

                            case -1: //Previous downvote
                                
                                switch (vote) {

                                    case 0: //revoke
                                    case -1: //revote which means revoke

                                        if (votingTracker != null) {
                                            comment.DownCount--;
                                            _db.CommentVoteTrackers.Remove(votingTracker);
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(0, REVOKE_MSG);
                                        }
                                        break;
                                    case 1:

                                        //change downvote to upvote
                                        if (votingTracker != null) {
                                            comment.UpCount++;
                                            comment.DownCount--;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.CreationDate = CurrentDate;
                                            
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(vote);
                                        }
                                        
                                        break;
                                }
                                    //SendVoteNotification(comment.Name, "downtoupvote");
                                break;

                        }
                        return response;
                    }
                
                    break;
                #endregion 
                #region Submission

                case ContentType.Submission:

                    var submission = _db.Submissions.FirstOrDefault(x => x.ID == id);


                    if (submission != null) {

                        //Run Rules Evaluation
                        VoatRulesEngine.Instance.Context.SubverseName = submission.Subverse;
                        VoatRulesEngine.Instance.Context.PropertyBag.Submission = submission;

                        outcome = VoatRulesEngine.Instance.IsSubmissionVoteAllowed(id, vote);
                        if (!outcome.IsAllowed) {
                            return new VoteResponse(ProcessResult.Denied, null, outcome.ToString());
                        }


                        // do not execute voting, subverse is in anonymized mode
                        if (submission.IsAnonymized) {
                            return VoteResponse.Ignored(0, "Subverse is anonymized, voting disabled");
                        }

                        //ignore votes if comment is users
                        if (String.Equals(submission.UserName, userName, StringComparison.OrdinalIgnoreCase)) { 
                            return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                        }
                        
                        var existingVote = 0;
                        
                        var existingCommentVote = _db.SubmissionVoteTrackers.FirstOrDefault(x => x.SubmissionID == id && x.UserName == userName);

                        if (existingCommentVote != null && existingCommentVote.VoteStatus.HasValue) {
                            existingVote = existingCommentVote.VoteStatus.Value;
                        }

                        // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                        if (existingVote == vote && !revokeOnRevote) {
                            return VoteResponse.Ignored(existingVote, "User has already voted this way");
                        }


                        VoteResponse response = new VoteResponse(ProcessResult.NotProcessed, 0, "Vote not processed.");
                        var votingSubmissionTracker = _db.SubmissionVoteTrackers.FirstOrDefault(x => x.SubmissionID == id && x.UserName == userName);


                        switch (existingVote) {
                            
                            case 0: //Never voted or No vote

                                switch (vote) { 
                                    case 0: //revoke

                                        response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");

                                        break;
                                    case 1:
                                    case -1:
                                        
                                        if (vote == 1) {
                                            submission.UpCount++;
                                        } else {
                                            //VoatRulesEngine.Instance.Context.SubverseName = comment.Message.Subverse;

                                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.DownVoteSubmission);
                                            if (outcome.IsDenied) {
                                                return VoteResponse.Denied(outcome.ToString());
                                            }

                                            //if (!Voat.UserHelper.CanUserDownVoteInSubverse(submission.Subverses, userName)) {
                                            //    return VoteResponse.Denied("Subverse MinCPP requirement not met for downvote.");
                                            //}
                                            submission.DownCount++;
                                        }
                                
                                        var t = new SubmissionVoteTracker {
                                            SubmissionID = id,
                                            UserName = userName,
                                            VoteStatus = vote,
                                            CreationDate = DateTime.Now
                                        };
                                
                                        _db.SubmissionVoteTrackers.Add(t);
                                        _db.SaveChanges();

                                        //SendVoteNotification(comment.Name, "upvote");
                                        response = VoteResponse.Success(vote);
                                        break;
                                }
                                break;
                            case 1: //Previous Upvote

                                switch (vote) {
                                    case 0: //revoke
                                    case 1: //revote which means revoke if we are here

                                        if (votingSubmissionTracker != null) {
                                            
                                            submission.UpCount--;

                                            _db.SubmissionVoteTrackers.Remove(votingSubmissionTracker);
                                        
                                            _db.SaveChanges();

                                        }

                                        response = response = VoteResponse.Success(0, REVOKE_MSG);

                                        break;
                                    case -1:
                                        //change upvote to downvote

                                        if (votingSubmissionTracker != null) {

                                            submission.UpCount--;
                                            submission.DownCount++;

                                            votingSubmissionTracker.VoteStatus = vote;
                                            votingSubmissionTracker.CreationDate = CurrentDate;
                                            
                                            _db.SaveChanges();

                                            response = VoteResponse.Success(vote);
                                        }
                                        break;
                                }

                                //SendVoteNotification(comment.Name, "downvote");
                                //ResetCommentVote(userWhichUpvoted, commentId);

                                break;

                            case -1: //Previous downvote

                                switch (vote) {

                                    case 0: //revoke
                                    case -1: //revote which means revoke if we are here
                                        // delete existing downvote

                                        if (votingSubmissionTracker != null) {
                                            submission.DownCount--;
                                            _db.SubmissionVoteTrackers.Remove(votingSubmissionTracker);
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(0, REVOKE_MSG);
                                        }
                                        break;
                                    case 1:

                                        //change downvote to upvote
                                        var votingTracker = _db.SubmissionVoteTrackers.FirstOrDefault(x => x.SubmissionID == id && x.UserName == userName);

                                        if (votingTracker != null) {
                                            submission.UpCount++;
                                            submission.DownCount--;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.CreationDate = CurrentDate; 
                                            
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(vote);
                                        }
                                        

                                        break;
                                }
                                    //SendVoteNotification(comment.Name, "downtoupvote");
                                break;

                        }
                        return response;
                    }
                
                    break;
                #endregion 
            }




            return VoteResponse.Denied();

        }


        public Submission GetSubmission(int submissionID) {

            var processor = new Action<Submission>(x => {
                    //x.Content = (formatMessageContent ? Utils.Formatting.FormatMessage(x.MessageContent) : x.MessageContent);
                    x.UserName = (x.IsAnonymized ? x.ID.ToString() : x.UserName);
                });

            return GetSubmission(submissionID, new Func<Models.Submission, Models.Submission>(x => x), processor);
        }
  
        public T GetSubmission<T>(int submissionID, Func<Submission, T> selector, Action<T> processor) {

            var query = (from x in _db.Submissions
                         where x.ID == submissionID
                              select x);

            var record = query.Select(selector).FirstOrDefault();

            Process(record, processor);

            return record;

        }
        public List<ApiSubscription> GetSubscriptions(string userName) {


            var subs = (from x in _db.SubverseSubscriptions
                         where x.UserName == userName
                         select new ApiSubscription() { Name = x.Subverse, Type = SubscriptionType.Subverse }).ToList();

            var sets = (from x in _db.UserSetSubscriptions
                        where x.UserName == userName
                        select new ApiSubscription() { Name = x.UserSet.Name, Type = SubscriptionType.Set }).ToList();

            subs.AddRange(sets);

            return subs;

        }
       
        public List<Submission> GetUserSubmissions(string subverse, string userName, SearchOptions options) {

            var selector = new Func<Submission, Submission>(x => x);
            var processor = new Action<Submission>(x => x.UserName = (x.IsAnonymized ? x.ID.ToString() : x.UserName));

            return GetUserSubmissions(subverse, userName, options, selector, processor);

        }
        public List<T> GetUserSubmissions<T>(string subverse, string userName, SearchOptions options, Func<Submission, T> selector, Action<T> processor) {
            
            //This is a near copy of GetSubmissions<T> 
            if (String.IsNullOrEmpty(userName)) {
                throw new VoatValidationException("A username must be provided.");
            }
            if (!String.IsNullOrEmpty(subverse) && !SubverseExists(subverse)) {
                throw new VoatValidationException("Subverse '{0}' doesn't exist.", subverse);
            }
            if (!UserHelper.UserExists(userName)) {
                throw new VoatValidationException("User does not exist.");
            }

            IQueryable<Submission> query;

            subverse = ToCorrectSubverseCasing(subverse);

            query = (from x in _db.Submissions
                     where (x.UserName == userName && !x.IsAnonymized)
                     && (x.Subverse == subverse || subverse == null)   
                     select x);

            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = query.Select(selector).ToList();

            Process(results, processor);

            return results;

        }
        
        public List<Submission> GetSubmissions(string subverse, SearchOptions options) {

            var selector = new Func<Submission, Submission>(x => x);
            var processor = new Action<Submission>(x => x.UserName = (x.IsAnonymized ? x.ID.ToString() : x.UserName));

            return GetSubmissions(subverse, options, selector, processor);

        }

        public List<T> GetSubmissions<T>(string subverse, SearchOptions options, Func<Submission, T> selector, Action<T> processor) {

            
            //System.Web.HttpContext.Current.Cache.Add("voat", o);


            if (String.IsNullOrEmpty(subverse)) {
                throw new VoatValidationException("A subverse must be provided.");
            }

            if (options == null) {
                options = new SearchOptions();
            }
            

            IQueryable<Submission> query;

            switch (subverse.ToLower()){

                //for *special* subverses, this is UNDONE
                case "_front":
                    if (User.Identity.IsAuthenticated && UserHelper.SubscriptionCount(User.Identity.Name) > 0) {
                        query = (from x in _db.Submissions
                                 join subscribed in _db.SubverseSubscriptions on x.Subverse equals subscribed.Subverse
                                 where subscribed.UserName == User.Identity.Name 
                                 select x);
                    } else {
                        //if no user, default to default
                        query = (from x in _db.Submissions
                                 join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
                                select x);
                    }
                    break;

                case "_default":

                    query = (from x in _db.Submissions
                             join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
                            select x);
                    break;
                case "_any":

                    query = (from x in _db.Submissions.Include("Subverses")
                             where !x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate
                             select x);
                    break;
                case "_all":
                case "all":

                    var nsfw = (User.Identity.IsAuthenticated ? UserHelper.AdultContentEnabled(User.Identity.Name) : false);

                    //v/all has certain conditions
                    //1. Only subs that have a MinCCP of zero
                    //2. Don't show private subs
                    //3. Don't show NSFW subs if nsfw isn't enabled in profile, if they are logged in 
                    //4. Don't show blocked subs if logged in // not implemented

                    query = (from x in _db.Submissions
                             where  x.Subverse1.MinCCPForDownvote == 0 
                                    && (!x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate)
                                    && (x.Subverse1.IsAdult && nsfw || !x.Subverse1.IsAdult)
                             select x);

                    break;
                //for regular subverse queries
                default:

                    if (!SubverseExists(subverse)) {
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
            var results = query.Select(selector).ToList();

            Process(results, processor);

            return results;
        }

        public Subverse GetSubverseInfo(string sub) {

            var submission = (from x in _db.Subverses
                              where x.Name == sub
                              select x).FirstOrDefault();

            if (submission != null) {
                submission.SideBar = Utilities.Formatting.FormatMessage(submission.SideBar, true);
            }

            return submission;
        }
        
        //public List<T> GetSubmissions<T>(string subverse, SearchOptions options, Func<Message, T> selector, Action<T> processor) {

        public T GetSubverseInfo<T>(string sub, Func<Subverse, T> selector, Action<T> processor) {

            var submission = (from x in _db.Subverses
                              where x.Name == sub
                              select x).FirstOrDefault();

          

            if (submission != null) {
                submission.SideBar = Utilities.Formatting.FormatMessage(submission.SideBar, true);
            }

            return selector.Invoke(submission);
        }


        
        public bool SubverseExists(string subverse) {

            return _db.Subverses.Any(x => x.Name == subverse);
        
        }
        public string ToCorrectSubverseCasing(string subverse) {
            if (!String.IsNullOrEmpty(subverse)) {
                var sub = _db.Subverses.FirstOrDefault(x => x.Name == subverse);
                return (sub == null ? null : sub.Name);
            } else {
                return null;
            }
        }
        public string ToCorrectUserNameCasing(string userName) {
            if (!String.IsNullOrEmpty(userName)) {
                return UserHelper.OriginalUsername(userName);
            } else {
                return null;
            }
        }
       

        [Authorize]
        public Submission PostSubmission(string subverse, UserSubmission submission) {


            //Validation stuff
            if (submission == null || !submission.HasState) {
                throw new VoatValidationException("The submission must not be null or have invalid state.");
            }
            if (String.IsNullOrEmpty(subverse)) {
                throw new VoatValidationException("A subverse must be provided.");
            }
            if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content)) {
                throw new VoatValidationException("Either a Url or Content must be provided.");
            }
            if (String.IsNullOrEmpty(submission.Title)) {
                throw new VoatValidationException("Submission must have a title.");
            }
            if (Submissions.ContainsUnicode(submission.Title)) {
                throw new VoatValidationException("Submission title can not contain Unicode characters.");
            }
            if (!SubverseExists(subverse)) {
                throw new VoatValidationException(String.Format("Subverse '{0}' does not exist.", subverse));
            }

            //The underlying table is designed incorrectly for this kind of data
            //but it will have to be changed at a later date, hence why the logic change between 
            //self posts and link posts below

            Submission m = new Submission();
            m.UserName = User.Identity.Name;
            m.CreationDate = CurrentDate;
            m.Subverse = ToCorrectSubverseCasing(subverse); // < Needs to be the same casing as EF fails if relational keys aren't cased identically (Thanks @Siege!)

            //1: Self Post, 2: Link Post
            m.Type = (String.IsNullOrEmpty(submission.Url) ? 1 : 2);


            if (m.Type == 1) {

                m.Title = submission.Title;
                m.Content = submission.Content;
                m.LinkDescription = null;

            } else {

                m.Title = null;
                m.Content = submission.Url;
                m.LinkDescription = submission.Title;

            }

            _db.Submissions.Add(m);

            _db.SaveChanges();

            return m;

        }
        [Authorize]
        public Submission UpdateSubmission(int submissionID, UserSubmission submission) {

            if (submission == null || !submission.HasState) {
                throw new VoatValidationException("The submission must not be null or have invalid state.");
            }

            //if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content)) {
            //    throw new VoatValidationException("Either a Url or Content must be provided.");
            //}

            var m = GetSubmission(submissionID);
                
            if (m == null) {
                throw new VoatNotFoundException(String.Format("Can't find submission with ID {0}", submissionID));
            } 
                
            if (m.UserName != User.Identity.Name){
                throw new VoatSecurityException(String.Format("Submission can not be edited by account"));
            }


            

            //only allow edits for self posts
            if (m.Type == 1) {
                m.Content = submission.Content ?? m.Content;
            }

            //allow edit of title if in 10 minute window
            if (CurrentDate.Subtract(m.CreationDate).TotalMinutes <= 10.0f) {

                if (!String.IsNullOrEmpty(submission.Title) && Utilities.Submissions.ContainsUnicode(submission.Title)) {
                    throw new VoatValidationException("Submission title can not contain Unicode characters.");
                }

                if (m.Type == 1) {
                    m.Title = (String.IsNullOrEmpty(submission.Title) ? m.Title : submission.Title);
                } else {
                    m.LinkDescription = (String.IsNullOrEmpty(submission.Title) ? m.LinkDescription : submission.Title);
                }
            }

            m.LastEditDate = CurrentDate;

            _db.SaveChanges();

            return m;

        }

        [Authorize]
        //LOGIC COPIED FROM SubmissionController.DeleteSubmission(int)
        public void DeleteSubmission(int submissionID) {

             var submissionToDelete = _db.Submissions.Find(submissionID);

             if (submissionToDelete != null) {
                 // delete submission if delete request is issued by submission author
                 if (submissionToDelete.UserName == User.Identity.Name) {
                     submissionToDelete.IsDeleted = true;

                     if (submissionToDelete.Type == 1) {
                         submissionToDelete.Content = "deleted by author at " + DateTime.Now;
                     } else {
                         submissionToDelete.Content = "http://voat.co";
                     }

                     // remove sticky if submission was stickied
                     var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionID);
                     if (existingSticky != null) {
                         _db.StickiedSubmissions.Remove(existingSticky);
                     }

                     _db.SaveChanges();
                 }

                 // delete submission if delete request is issued by subverse moderator
                 else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse)) {
                     // mark submission as deleted
                     submissionToDelete.IsDeleted = true;

                     // move the submission to removal log
                     var removalLog = new SubmissionRemovalLog {
                         SubmissionID = submissionToDelete.ID,
                         Moderator = User.Identity.Name,
                         Reason = "This feature is not yet implemented",
                         CreationDate = DateTime.Now
                     };

                     _db.SubmissionRemovalLogs.Add(removalLog);

                    // notify submission author that his submission has been deleted by a moderator
                    var message =
                        "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.ID + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " at " + DateTime.Now + "  " + Environment.NewLine +
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
                     if (existingSticky != null) {
                         _db.StickiedSubmissions.Remove(existingSticky);
                     }

                     _db.SaveChanges();
                 } else {
                     throw new VoatSecurityException("User doesn't have permission to delete submission.");
                 }
             }
        }
        
        public List<Models.Comment> GetComments(int submissionID, SearchOptions options) {
            return GetComments(submissionID, options, new Func<Models.Comment, Models.Comment>(x => x), new Action<Models.Comment>(x => x.UserName = (x.IsAnonymized ? x.ID.ToString() : x.UserName)));
        }

        //public List<T> GetComments<T>(int submissionID, SearchOptions options, Func<Models.Comment, T> selector, Action<T> processor) {
        //    return GetComments(submissionID, options, selector, processor);
        //}

        public List<T> GetUserComments<T>(string userName, SearchOptions options, Func<Models.Comment, T> selector, Action<T> processor) {

            if (String.IsNullOrEmpty(userName)) {
                throw new VoatValidationException("A user name must be provided.");
            }
            if (!UserHelper.UserExists(userName)) {
                throw new VoatValidationException("User '{0}' does not exist.", userName);
            }

            var query = (from x in _db.Comments
                         where
                            (x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                         //&& (x.Name == userName && !x.Anonymized || userName == null)
                         select x);

            query = ApplyCommentSearch(options, query);

            //execute query
            var results = query.Select(selector).ToList();

            if (results != null && processor != null) {
                results.ForEach(processor);
            }

            return results;
        
        }
        public List<T> GetComments<T>(int? submissionID, SearchOptions options, Func<Models.Comment, T> selector, Action<T> processor) {


            var query = (from x in _db.Comments.Include("Message.Subverses")
                         where
                            (!x.Submission.Subverse1.IsPrivate && submissionID == null) && //got to filter out comments from private subs if calling from the streaming endpoint
                            (x.SubmissionID == submissionID || submissionID == null)
                         select x);

            query = ApplyCommentSearch(options, query);

            //execute query
            var results = query.Select(selector).ToList();

            if (results != null && processor != null) {
                results.ForEach(processor);
            }

           return results;
        }
        //This is the new process to retrieve comments.
        public List<T> GetCommentTree<T>(int submissionID, int? depth, int? parentID, Func<Models.usp_CommentTree_Result, T> selector, Action<T> processor) {
            
            if (depth.HasValue && depth < 0) {
                depth = null;
            }

            var commentTree = _db.usp_CommentTree(submissionID, depth, parentID);
            
            //execute query
            var results = commentTree.Select(selector).ToList();

            if (results != null && processor != null) {
                results.ForEach(processor);
            }

            return results;
        }

        public T GetComment<T>(int commentID, Func<Models.Comment, T> selector, Action<T> processor) {

            var direct = _db.Comments.Where(x => x.ID == commentID);
            
            T record = direct.Select(selector).FirstOrDefault();

            Process(record, processor);

            return record;

        }
        public Comment GetComment(int commentID) {

            return GetComment(commentID, new Func<Comment, Comment>(x => x), null);

        }
        [Authorize]
        public void DeleteComment(int commentID) {
            
            var comment = _db.Comments.Find(commentID);

            if (comment != null) {
                var commentSubverse = comment.Submission.Subverse;
                // delete comment if the comment author is currently logged in user
                if (comment.UserName == User.Identity.Name) {
                    comment.Content = "deleted by author at " + CurrentDate.ToLongDateString();
                    comment.IsDeleted = true;
                }// delete comment if delete request is issued by subverse moderator
                else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, commentSubverse)) {
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
                } else {
                    var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                    ex.Data["CommentID"] = commentID;
                    throw ex;
                }
                _db.SaveChanges();
            }
        }
        [Authorize]
        public Comment UpdateComment(int commentID, string comment) {

            var current = _db.Comments.Find(commentID);

            if (current != null) {

                if (current.UserName.Trim() == User.Identity.Name) {

                    current.LastEditDate = CurrentDate;

                    var escapedCommentContent = WebUtility.HtmlEncode(comment);
                    current.Content = escapedCommentContent;

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave)) {
                        current.Content = ContentProcessor.Instance.Process(current.Content, ProcessingStage.InboundPreSave, current);
                    }

                    _db.SaveChanges();

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave)) {
                        ContentProcessor.Instance.Process(current.Content, ProcessingStage.InboundPostSave, current);
                    }

                } else {
                    var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                    ex.Data["UserName"] = User.Identity.Name;
                    ex.Data["CommentID"] = commentID;
                    throw ex;
                }
            } else {
                throw new VoatNotFoundException("Can not find comment with ID {0}", commentID);
            }
        
            return current;

        }
        [Authorize]
        public Comment PostCommentReply(int parentCommentID, string comment){
            var c = _db.Comments.Find(parentCommentID);
            if (c == null) {
                throw new VoatNotFoundException("Can not find parent comment with id {0}", parentCommentID.ToString());
            }
            var submissionid = c.SubmissionID;
            return PostComment(submissionid.Value, parentCommentID, comment);

        }
        [Authorize]
        public Comment PostComment(int submissionID, int? parentCommentID, string comment) {

            var submission = _db.Submissions.Find(submissionID);

            if (submission == null) {
                throw new VoatNotFoundException("submissionID", submissionID, "Can not find submission");
            }

            var subverse = _db.Subverses.Where(x => x.Name == submission.Subverse).FirstOrDefault();
           
            var c = new Comment();
            c.CreationDate = DateTime.Now;
            c.UserName = User.Identity.Name;
            c.Content = comment;
            c.ParentID = (parentCommentID > 0 ? parentCommentID : (int?)null);
            c.SubmissionID = submissionID;
            c.Votes = 0;
            c.UpCount = 0;
            c.IsAnonymized = (submission.IsAnonymized || subverse.IsAnonymized);
            

            // check if author is banned, don't save the comment or send notifications if true
            if (!UserHelper.IsUserGloballyBanned(User.Identity.Name) && !UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse)) {
                _db.Comments.Add(c);

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave)) {
                    c.Content = ContentProcessor.Instance.Process(c.Content, ProcessingStage.InboundPreSave, c);
                }

                _db.SaveChangesAsync();

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave)) {
                    ContentProcessor.Instance.Process(c.Content, ProcessingStage.InboundPostSave, c);
                }

                // send comment reply notification to parent comment author if the comment is not a new root comment
                NotificationManager.SendCommentNotification(c, null);
            }

            return c;  

        }



        public ApiUserInfo GetUserInfo(string userName) {
            
            //copypasta from LegacyWebApiController.UserInfo
            //This method makes multiple calls to the db, need to reduce overhead eventually
            if (userName != "deleted" && !UserHelper.UserExists(userName) || userName == "deleted") {
                return null;
            }

            var userInfo = new ApiUserInfo();


            var userBadges = (from x in _db.UserBadges
                              join b in _db.Badges on x.BadgeID equals b.ID
                              where x.UserName == userName
                              select new ApiUserBadge() {
                                  Awarded = x.CreationDate,
                                  BadgeName = b.Name,
                                  BadgeTitle = b.Title,
                                  BadgeGraphics = b.Graphic,
                              }
                              ).ToList();

            
            userInfo.UserName = UserHelper.OriginalUsername(userName);
            userInfo.CommentPoints = UserContributionPoints(userName, ContentType.Comment);
            userInfo.SubmissionPoints = UserContributionPoints(userName, ContentType.Submission);

            if (User.Identity.IsAuthenticated)
            {
                userInfo.SubmissionVoting = UserVotingBehavior(userName, ContentType.Submission);
                userInfo.CommentVoting = UserVotingBehavior(userName, ContentType.Comment);
            }
            userInfo.RegistrationDate = UserHelper.GetUserRegistrationDateTime(userName);
            userInfo.Badges = userBadges;
            userInfo.Bio = UserHelper.UserShortbio(userName);
            userInfo.ProfilePicture = VoatPathHelper.AvatarPath(userName, true, true);

            return userInfo;
        }
        public UserPreference GetUserPreferences(string userName) {

            var selector = new Func<UserPreference, UserPreference>(x => x);

            return GetUserPreferences(userName, selector, null);

        }

        public T GetUserPreferences<T>(string userName, Func<Models.UserPreference, T> selector, Action<T> processor) {

            var query = _db.UserPreferences.Where(x => (x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)));

            var results = query.Select(selector).FirstOrDefault();

            Process(results, processor);

            return results;
                    
        }

        private static IQueryable<Submission> ApplySubmissionSearch(SearchOptions options, IQueryable<Submission> query) {

            //HACK: Warning, Super hacktastic
            if (!String.IsNullOrEmpty(options.Search)){

                //WARNING: This is a quickie that views spaces as AND conditions in a search.
                List<string> keywords = null;
                if (options.Search.Contains(" ")) { 
                    keywords = new List<string>(options.Search.Split(' '));    
                } else {
                    keywords = new List<string>(new string[] {options.Search});
                }

                keywords.ForEach(x => {
                    query = query.Where(m => m.Title.Contains(x) || m.Content.Contains(x) || m.LinkDescription.Contains(x));
                });
                
            }
            if (options.StartDate.HasValue) {
                query = query.Where(x => x.CreationDate >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue) {
                query = query.Where(x => x.CreationDate <= options.EndDate.Value);
            }
            //Search Options
            switch (options.Sort) {

                case SortAlgorithm.Hot:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Rank);
                    } else {
                        query = query.OrderByDescending(x => x.Rank);
                    }
                    break;

                case SortAlgorithm.New:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.CreationDate);
                    } else {
                        query = query.OrderByDescending(x => x.CreationDate);
                    }
                    break;

                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.UpCount); 
                    } else {
                        query = query.OrderByDescending(x => x.UpCount);
                    }
                    break;
                case SortAlgorithm.Viewed:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Views);
                    } else {
                        query = query.OrderByDescending(x => x.Views);
                    }
                    break;
                case SortAlgorithm.Discussed:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Comments.Count);
                    } else {
                        query = query.OrderByDescending(x => x.Comments.Count);
                    }
                    break;
                case SortAlgorithm.Active:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                    } else {
                        query = query.OrderByDescending(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                    }
                    break;
                case SortAlgorithm.Bottom:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.DownCount);
                    } else {
                        query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;

            }

            query = query.Skip(options.Index).Take(options.Count);
            return query;
        }


        private static IQueryable<Comment> ApplyCommentSearch(SearchOptions options, IQueryable<Comment> query) {

            if (!String.IsNullOrEmpty(options.Search)) {
                //TODO: This is a hack that views Spaces as AND conditions in a search.
                List<string> keywords = null;
                if (!String.IsNullOrEmpty(options.Search) && options.Search.Contains(" ")) {
                    keywords = new List<string>(options.Search.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries));
                } else {
                    keywords = new List<string>(new string[] { options.Search });
                }

                keywords.ForEach(x => {
                    query = query.Where(m => m.Content.Contains(x));
                });

            }
            if (options.StartDate.HasValue) {
                query = query.Where(x => x.CreationDate >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue) {
                query = query.Where(x => x.CreationDate <= options.EndDate.Value);
            }
            //TODO: Implement Depth in Comment Table
            //if (options.Depth > 0) {
            //    query = query.Where(x => 1 == 1);
            //}

            switch (options.Sort) {
                case SortAlgorithm.Hot:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => (x.UpCount - x.DownCount));
                    } else {
                        query = query.OrderByDescending(x => (x.UpCount - x.DownCount));
                    }
                    break;
                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.UpCount);
                    } else {
                        query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;
                case SortAlgorithm.New:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.CreationDate);
                    } else {
                        query = query.OrderByDescending(x => x.CreationDate);
                    }
                    break;
            }

            query = query.Skip(options.Index).Take(options.Count);
            
            
            return query;
        }

       /// <summary>
       /// Save Comments and Submissions toggle.
       /// </summary>
       /// <param name="type">The type of content in which to save</param>
       /// <param name="ID">The ID of the item in which to save</param>
        /// <param name="forceAction">Forces the Save function to operate as a Save only or Unsave only rather than a toggle. If true, will only save if it hasn't been previously saved, if false, will only remove previous saved entry, if null (default) will function as a toggle.</param>
        /// <returns>The end result if the item is saved or not. True if saved, false if not saved.</returns>
        public bool Save(ContentType type, int ID, bool? forceAction = null) {

            //TODO: These save trackers should be stored in a single table in SQL. Two tables for such similar information isn't ideal... mmkay. Makes querying nasty. 
            //TODO: There is a potential issue with this code. There is no validation that the ID belongs to a comment or a submission. This is nearly impossible to determine anyways but it's still an issue.
            string currentUserName = User.Identity.Name;
            bool isSaved = false;

            switch (type) {
                case ContentType.Comment:
                    
                    var c = _db.CommentSaveTrackers.FirstOrDefault(x => x.CommentID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    
                    if (c == null && (forceAction == null || forceAction.HasValue && forceAction.Value)) {

                        c = new CommentSaveTracker() { CommentID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.CommentSaveTrackers.Add(c);
                        isSaved = true;

                    } else if (c != null && (forceAction == null || forceAction.HasValue && !forceAction.Value)) {
                        
                        _db.CommentSaveTrackers.Remove(c);
                        isSaved = false;
                   
                    }
                    _db.SaveChanges();

                    break;
                case ContentType.Submission:
                    
                    var s = _db.SubmissionSaveTrackers.FirstOrDefault(x => x.SubmissionID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    if (s == null && (forceAction == null || forceAction.HasValue && forceAction.Value)) {
                        
                        s = new SubmissionSaveTracker() { SubmissionID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.SubmissionSaveTrackers.Add(s);
                        isSaved = true;

                    } else if (s != null && (forceAction == null || forceAction.HasValue && !forceAction.Value)) {

                        _db.SubmissionSaveTrackers.Remove(s);
                        isSaved = false;

                    }
                    _db.SaveChanges();

                    break;
            }

            return (forceAction.HasValue ? forceAction.Value : isSaved);

        }

        #region Api Keys


        //public bool IsApiKeyValid(string apiPublicKey)
        //{

        //    var key = GetApiKey(apiPublicKey);

        //    if (key != null && key.IsActive)
        //    {

        //        //TODO: This needs to be non-blocking and non-queued. If 20 threads with same apikey are accessing this method at once we don't want to perform 20 updates on record.  
        //        //keep track of last access date
        //        key.LastAccessDate = CurrentDate;
        //        _db.SaveChanges();

        //        return true;
        //    }

        //    return false;
        //}

        //public ApiClient GetApiKey(string apiPublicKey) {

        //    var result = (from x in this._db.ApiClients
        //                  where x.ClientPublicKey == apiPublicKey
        //                  select x).FirstOrDefault();
        //    return result;
        //}

        //[Authorize]
        //public IEnumerable<ApiClient> GetApiKeys(string userName) {

        //    var result = from x in this._db.ApiClients
        //                 where x.UserName == userName
        //                 orderby x.CreationDate descending
        //                 select x;
        //    return result.ToList();
        //}

        //[Authorize]
        //public ApiThrottlePolicy GetApiThrottlePolicy(int throttlePolicyID) {
        //    var result = from policy in _db.ApiThrottlePolicies
        //                 where policy.ID == throttlePolicyID
        //                 select policy;

        //    return result.FirstOrDefault();
        //}

        //[Authorize]
        //public List<KeyValuePair<string, string>> GetApiClientKeyThrottlePolicies() {

        //    List<KeyValuePair<string, string>> policies = new List<KeyValuePair<string, string>>();

        //    var result = from client in this._db.ApiClients
        //                 join policy in _db.ApiThrottlePolicies on client.ApiThrottlePolicyID equals policy.ID
        //                 where client.IsActive == true
        //                 select new { client.ClientPublicKey, policy.Policy };

        //    foreach (var policy in result) {
        //        policies.Add(new KeyValuePair<string, string>(policy.ClientPublicKey, policy.Policy));
        //    }

        //    return policies;
        //}
        //[Authorize]
        ////public void CreateApiKey(string name, string description, string url) {

        ////    ApiClient c = new ApiClient();
        ////    c.IsActive = true;
        ////    c.AppAboutUrl = url;
        ////    c.AppDescription = description;
        ////    c.AppName = name;
        ////    c.UserName = User.Identity.Name;
        ////    c.CreationDate = CurrentDate;

        ////    byte[] tempKey = new byte[16];
        ////    RandomNumberGenerator.Create().GetBytes(tempKey);
        ////    c.ClientPublicKey = Convert.ToBase64String(tempKey);

        ////    tempKey = new byte[64];
        ////    RandomNumberGenerator.Create().GetBytes(tempKey);
        ////    c.ClientPrivateKey = Convert.ToBase64String(tempKey);

        ////    _db.ApiClients.Add(c);
        ////    _db.SaveChanges();

        ////}


        //[Authorize]
        //public void DeleteApiKey(int id) {
        //    //Only allow users to delete ApiKeys if they IsActive == 1
        //    var apiKey = (from x in _db.ApiClients
        //                  where x.ID == id && x.UserName == User.Identity.Name && x.IsActive == true
        //                  select x).FirstOrDefault();

        //    if (apiKey != null) {
        //        _db.ApiClients.Remove(apiKey);
        //        _db.SaveChanges();
        //    }
        //}

        #endregion

        #region UserMessages

        [Authorize]
        public void SaveUserPrefernces(ApiUserPreferences preferences) {

            SaveUserPrefernces(new UserPreference() {
                //Avatar = preferences.Avatar,
                //Shortbio = preferences.Bio,
                OpenInNewWindow = preferences.ClickingMode,
                DisableCSS = preferences.DisableCustomCSS,
                EnableAdultContent = preferences.EnableAdultContent,
                Language = preferences.Language,
                NightMode = preferences.EnableNightMode,
                DisplaySubscriptions = preferences.PubliclyShowSubscriptions,
                DisplayVotes = preferences.PubliclyDisplayVotes
            });

        }

        [Authorize]
        public void SaveUserPrefernces(UserPreference preferences) {
            var p = (from x in _db.UserPreferences
                     where x.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();

            if (p == null) {
                p = new UserPreference();
                p.UserName = User.Identity.Name;
            }

            if (!String.IsNullOrEmpty(preferences.Avatar)) {
                p.Avatar = preferences.Avatar;
            }
            if (!String.IsNullOrEmpty(preferences.Bio)) {
                p.Bio = preferences.Bio;
            }
            if (!String.IsNullOrEmpty(preferences.Language)) {
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
        //[ApiModelStateValidation]
        public void SendMessageReply(int id, string value) {

            //throw new NotImplementedException();

            //THIS WILL BE WAY EASIER TO IMPLEMENT AFTER THE MESSAGE TABLES ARE MERGED.
            //find message
            var c = (from x in _db.CommentReplyNotifications
                     where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (c != null) {
                PostCommentReply(c.CommentID, value);
                return;
            }

            var sub = (from x in _db.SubmissionReplyNotifications
                       where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (sub != null) {
                PostComment(sub.SubmissionID, sub.CommentID, value);
                return;
            }
            var pm = (from x in _db.PrivateMessages
                      where x.ID == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (pm != null) {
                SendMessage(new ApiSendUserMessage() { Subject = sub.Subject, Message = value, Recipient = sub.Sender });
                return;
            }

            throw new VoatNotFoundException("Message to reply was not found");
        }

        [Authorize]
        //[ApiModelStateValidation]
        public void SendMessage(ApiSendUserMessage sendUserMessage) {

            //if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (sendUserMessage.Recipient == null || sendUserMessage.Subject == null || sendUserMessage.Message == null) {
                return;
            }
            if (UserHelper.IsUserGloballyBanned(User.Identity.Name)) {
                return;
            }


            // check if recipient exists
            if (UserHelper.UserExists(sendUserMessage.Recipient)) {



                PrivateMessage msg = new PrivateMessage();

                // send the message
                msg.Recipient = sendUserMessage.Recipient;
                msg.Body = sendUserMessage.Message;
                msg.Subject = sendUserMessage.Subject;
                msg.CreationDate = CurrentDate;
                msg.Sender = User.Identity.Name;
                msg.IsUnread = true;

                _db.PrivateMessages.Add(msg);

                try {
                    _db.SaveChanges();

                    //TODO: Implement this logic outside of the Repository

                    //// get count of unread notifications
                    //int unreadNotifications = Utils.User.UnreadTotalNotificationsCount(msg.Recipient);
                    //// send SignalR realtime notification to recipient
                    //var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                    //hubContext.Clients.User(privateMessage.Recipient).setNotificationsPending(unreadNotifications);

                } catch (Exception ex) {
                    throw ex;
                }
            } else {
                throw new VoatNotFoundException("User {0} does not exist.", sendUserMessage.Recipient);
            }
        }
       

        [Authorize]
        public List<UserMessage> GetUserMessages(MessageType type, MessageState state, bool markAsRead = true) {

            //This ENTIRE routine will need to be refactored once the message tables are merged. THIS SHOULD BE HIGH PRIORITY AS THIS IS HACKY HACKSTERISTIC
            List<UserMessage> messages = new List<UserMessage>();
            
            if ((type & MessageType.Inbox) > 0 || (type & MessageType.Sent) > 0) {
                var msgs = (from x in _db.PrivateMessages
                            where (
                                ((x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type | MessageType.Inbox) > 0) ||
                                (x.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type | MessageType.Sent) > 0)) && 
                                x.IsUnread == (state == MessageState.Unread))
                            select new UserMessage() {
                                ID = x.ID,
                                CommentID = null,
                                SubmissionID = null,
                                Subverse = null,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = x.Subject,
                                Content = x.Body,
                                Unread = x.IsUnread,
                                Type = (x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) ? MessageType.Inbox : MessageType.Sent),
                                SentDate = x.CreationDate
                            });
                messages.AddRange(msgs.ToList());
            }
            //Comment Replies, Mention Replies
            if ((type & MessageType.Comment) > 0 || (type & MessageType.Mention) > 0) {
                var msgs = (from x in _db.CommentReplyNotifications
                            join s in _db.Submissions on x.SubmissionID equals s.ID
                            join c in _db.Comments on x.CommentID equals c.ID into commentJoin
                            from comment in commentJoin.DefaultIfEmpty()
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                                && x.IsUnread == (state == MessageState.Unread)
                                )
                            select new UserMessage() {
                                ID = x.ID,
                                CommentID = (x.CommentID > 0 ? x.CommentID : (int?)null),
                                SubmissionID = x.SubmissionID,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = s.Title,
                                Content = (comment == null ? s.Content : comment.Content),
                                Unread = x.IsUnread,
                                Type = (s.Content.Contains(User.Identity.Name) ? MessageType.Mention : MessageType.Comment), //TODO: Need to determine if comment reply or mention
                                SentDate = x.CreationDate
                            });

                
                if ((type & MessageType.Comment) > 0 && (type & MessageType.Mention) > 0) {
                    //this is both
                    messages.AddRange(msgs.ToList());
                } else if ((type & MessageType.Mention) > 0) {
                    messages.AddRange(msgs.ToList().FindAll(x => x.Type == MessageType.Mention));
                } else {
                    messages.AddRange(msgs.ToList().FindAll(x => x.Type == MessageType.Comment));
                }
            }

            //Post Replies
            if ((type & MessageType.Submission) > 0) {
                var msgs = (from x in _db.SubmissionReplyNotifications
                            join c in _db.Comments on x.CommentID equals c.ID
                            join s in _db.Submissions on c.SubmissionID equals s.ID
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) &&
                                x.IsUnread == (state == MessageState.Unread))
                            select new UserMessage() {
                                ID = x.ID,
                                CommentID = x.CommentID,
                                SubmissionID = x.SubmissionID,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = (s.Type == 1 ? s.LinkDescription : s.Title),
                                Content = c.Content,
                                Unread = x.IsUnread,
                                Type = MessageType.Submission,
                                SentDate = x.CreationDate
                            });
                messages.AddRange(msgs.ToList());
            }

            //mark as read, super hacky until message tables are merged
            if (markAsRead) {
                new Task(() => {
                    foreach (var msg in messages) {
                        if (msg.Unread) {
                            switch (msg.Type) {
                                case MessageType.Comment:
                                case MessageType.Mention:
                                    var m = _db.CommentReplyNotifications.First(x => x.ID == msg.ID);
                                    m.IsUnread = false;
                                    break;
                                case MessageType.Inbox:
                                    var p = _db.PrivateMessages.First(x => x.ID == msg.ID);
                                    p.IsUnread = false;

                                    break;
                                case MessageType.Submission:
                                    var s = _db.SubmissionReplyNotifications.First(x => x.ID == msg.ID);
                                    s.IsUnread = false;
                                    break;
                            }
                        }
                    }
                    _db.SaveChanges();
                }).Start();
            }

            return messages;

        }
        #endregion

        #region User Related Functions

        public Score UserVotingBehavior(string userName, ContentType type = ContentType.Comment | ContentType.Submission, TimeSpan? span = null)
        {

            Score vb = new Score();
            
            if ((type & ContentType.Comment) > 0) {
                var c = UserVotingBehaviorComments(userName, span);
                vb.Combine(c);
            }
            if ((type & ContentType.Submission) > 0) {
                var c = UserVotingBehaviorSubmissions(userName, span);
                vb.Combine(c);
            }

            return vb;
        }

        private Score UserVotingBehaviorSubmissions(string userName, TimeSpan? span = null)
        {

            DateTime? compareDate = null;
            if (span.HasValue) {
                compareDate = CurrentDate.Subtract(span.Value);
            }


            var result = (from x in _db.SubmissionVoteTrackers
                          where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                          && ((compareDate.HasValue && x.CreationDate >= compareDate) || !compareDate.HasValue)
                          group x by x.VoteStatus into v
                          select new {
                              key = v.Key,
                              votes = v.Count()
                          });

            Score vb = new Score();

            if (result != null) {
                foreach (var r in result) {
                    if (r.key.HasValue) {
                        if (r.key.Value == 1) {
                            vb.UpCount = r.votes;
                        } else {
                            vb.DownCount = r.votes;
                        }
                    }
                }
            }

            return vb;
            //OLD CODE - MULTIPLE QUERIES 

            //// get voting habits
            //var submissionUpvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
            //var submissionDownvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

            //var totalSubmissionVotes = submissionUpvotes + submissionDownvotes;

            //// downvote ratio
            //var downvotePercentage = (double)submissionDownvotes / totalSubmissionVotes * 100;

            //// upvote ratio
            //var upvotePercentage = (double)submissionUpvotes / totalSubmissionVotes * 100;

            //return downvotePercentage > upvotePercentage;


            //return null;

        }


        private Score UserVotingBehaviorComments(string userName, TimeSpan? span = null)
        {

            DateTime? compareDate = null;
            if (span.HasValue) {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.CommentVoteTrackers
                          where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                           && ((compareDate.HasValue && x.CreationDate >= compareDate) || !compareDate.HasValue)
                          group x by x.VoteStatus into v
                          select new {
                              key = v.Key,
                              votes = v.Count()
                          });

            Score vb = new Score();

            if (result != null) {
                foreach (var r in result) {
                    if (r.key.HasValue) {
                        if (r.key.Value == 1) {
                            vb.UpCount = r.votes;
                        } else {
                            vb.DownCount = r.votes;
                        }
                    }
                }
            }

            return vb;
          
        }
        
        public int UserCommentCount(string userName, TimeSpan? span, string subverse = null) {

            DateTime? compareDate = null;
            if (span.HasValue) {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.Comments.Include("Message")
                          where 
                            x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                            && (x.Submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.CreationDate >= compareDate)
                          select x).Count();
            return result;
        }

        public int UserSubmissionCount(string userName, TimeSpan? span, SubmissionType? type = null, string subverse = null) {

            DateTime? compareDate = null;
            if (span.HasValue) {
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

            if ((type & ContentType.Comment) > 0) {
                var totals = (from x in _db.Comments.Include("Message")
                              where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.UserName into y
                              select new {
                                  up = y.Sum(ups => ups.UpCount),
                                  down = y.Sum(downs => downs.DownCount)
                              }).FirstOrDefault();

                if (totals != null) {
                    s.Combine(new Score() { UpCount = (int)totals.up, DownCount = (int)totals.down });
                }
            }

            if ((type & ContentType.Submission) > 0) {
                var totals = (from x in _db.Submissions
                              where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.UserName into y
                              select new {
                                  up = y.Sum(ups => ups.UpCount),
                                  down = y.Sum(downs => downs.DownCount)
                              }).FirstOrDefault();
                if (totals != null) {
                    s.Combine(new Score() { UpCount = (int)totals.up, DownCount = (int)totals.down });
                }
            }

            return s;

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

        #region Misc

        //public Models.ExceptionLog Log(ExceptionLog log) {

        //    var newLog = _db.ExceptionLogs.Add(log);
        //    _db.SaveChanges();
        //    return newLog;

        //}
        private void Process<T>(T record, Action<T> processor) {
            if (record != null && processor != null) {
                processor.Invoke(record);
            }
        }

        private void Process<T>(List<T> records, Action<T> processor) {
            if (records != null && processor != null) {
                records.ForEach(processor);
            }
        }



        public static DateTime CurrentDate {
            //This will need to be UTC in the future
            get {
                //return DateTime.Now;
                return DateTime.UtcNow;
            }
        }


        public void Dispose() {
            Dispose(false);
        }

        ~DataGateway() {
            Dispose(true);
        }

        protected void Dispose(bool gcCalling) {
            if (_db != null) {
                _db.Dispose();
            }
            if (!gcCalling) {
                System.GC.SuppressFinalize(this);
            }
        }

        #endregion

    }
}