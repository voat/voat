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
                           where x.Id == commentID
                           select x.Message.Subverse).FirstOrDefault();
            return subname;
        }

        public string SubverseForSubmission(int submissionID) {
            var subname = (from x in _db.Messages
                           where x.Id == submissionID
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

                    var comment = _db.Comments.Include("Message").FirstOrDefault(x => x.Id == id);
                    VoatRulesEngine.Instance.Context.SubverseName = comment.Message.Subverse;

                    if (comment != null) {

                        // do not execute voting, subverse is in anonymized mode
                        if (comment.Message.Anonymized) {
                            return VoteResponse.Ignored(0, "Subverse is anonymized, voting disabled");
                        }

                        //ignore votes if comment is users
                        if (String.Equals(comment.Name, userName, StringComparison.InvariantCultureIgnoreCase)) { 
                            return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                        }
                        
                        var existingVote = 0;
                        
                        var existingCommentVote = _db.Commentvotingtrackers.FirstOrDefault(x => x.CommentId == id && x.UserName == userName);

                        if (existingCommentVote != null && existingCommentVote.VoteStatus.HasValue) {
                            existingVote = existingCommentVote.VoteStatus.Value;
                        }

                        // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                        if (existingVote == vote && !revokeOnRevote) {
                            return VoteResponse.Ignored(existingVote, "User has already voted this way.");
                        }


                        VoteResponse response = new VoteResponse(ProcessResult.NotProcessed, 0, "Vote not processed.");
                        var votingTracker = _db.Commentvotingtrackers.FirstOrDefault(b => b.CommentId == id && b.UserName == userName);

                        switch (existingVote) {
                            

                            case 0: //Never voted or No vote

                                switch (vote) { 
                                    case 0:
                                        response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");
                                        break;
                                    case 1:
                                    case -1:
                                        
                                        if (vote == 1) {
                                            comment.Likes++;
                                        } else {

                                            //VoatRulesEngine.Instance.Context.SubverseName = comment.Message.Subverse;

                                            //outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.DownVoteComment);
                                            //if (outcome.IsDenied) {
                                            //    return VoteResponse.Denied(outcome.ToString());
                                            //}

                                            //if (!Voat.UserHelper.CanUserDownVoteInSubverse(comment.Message.Subverses, userName)) {
                                            //    return VoteResponse.Denied("Subverse MinCPP requirement not met for downvote.");
                                            //}

                                            comment.Dislikes++;
                                        }
                                
                                        var newVotingTracker = new Commentvotingtracker {
                                            CommentId = id,
                                            UserName = userName,
                                            VoteStatus = vote,
                                            Timestamp = DateTime.Now
                                        };
                                
                                        _db.Commentvotingtrackers.Add(newVotingTracker);
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

                                            comment.Likes--;

                                            _db.Commentvotingtrackers.Remove(votingTracker);
                                        
                                            _db.SaveChanges();

                                            response = VoteResponse.Success(0, REVOKE_MSG);

                                        }
                                        break;
                                    case -1:
                                        //change upvote to downvote
                                        

                                        if (votingTracker != null) {

                                            comment.Likes--;
                                            comment.Dislikes++;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.Timestamp = CurrentDate;                                             
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
                                            comment.Dislikes--;
                                            _db.Commentvotingtrackers.Remove(votingTracker);
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(0, REVOKE_MSG);
                                        }
                                        break;
                                    case 1:

                                        //change downvote to upvote
                                        if (votingTracker != null) {
                                            comment.Likes++;
                                            comment.Dislikes--;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.Timestamp = CurrentDate;
                                            
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

                    var submission = _db.Messages.FirstOrDefault(x => x.Id == id);


                    if (submission != null) {

                        //Run Rules Evaluation
                        VoatRulesEngine.Instance.Context.SubverseName = submission.Subverse;
                        VoatRulesEngine.Instance.Context.PropertyBag.Submission = submission;

                        outcome = VoatRulesEngine.Instance.IsSubmissionVoteAllowed(id, vote);
                        if (!outcome.IsAllowed) {
                            return new VoteResponse(ProcessResult.Denied, null, outcome.ToString());
                        }


                        // do not execute voting, subverse is in anonymized mode
                        if (submission.Anonymized) {
                            return VoteResponse.Ignored(0, "Subverse is anonymized, voting disabled");
                        }

                        //ignore votes if comment is users
                        if (String.Equals(submission.Name, userName, StringComparison.OrdinalIgnoreCase)) { 
                            return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                        }
                        
                        var existingVote = 0;
                        
                        var existingCommentVote = _db.Votingtrackers.FirstOrDefault(x => x.MessageId == id && x.UserName == userName);

                        if (existingCommentVote != null && existingCommentVote.VoteStatus.HasValue) {
                            existingVote = existingCommentVote.VoteStatus.Value;
                        }

                        // do not execute voting, user has already up/down voted item and is submitting a vote that matches their existing vote
                        if (existingVote == vote && !revokeOnRevote) {
                            return VoteResponse.Ignored(existingVote, "User has already voted this way");
                        }


                        VoteResponse response = new VoteResponse(ProcessResult.NotProcessed, 0, "Vote not processed.");
                        var votingSubmissionTracker = _db.Votingtrackers.FirstOrDefault(x => x.MessageId == id && x.UserName == userName);


                        switch (existingVote) {
                            
                            case 0: //Never voted or No vote

                                switch (vote) { 
                                    case 0: //revoke

                                        response = VoteResponse.Ignored(0, "A revoke on an unvoted item has opened a worm hole! Run!");

                                        break;
                                    case 1:
                                    case -1:
                                        
                                        if (vote == 1) {
                                            submission.Likes++;
                                        } else {
                                            //VoatRulesEngine.Instance.Context.SubverseName = comment.Message.Subverse;

                                            outcome = VoatRulesEngine.Instance.EvaluateRuleSet(RuleScope.DownVoteSubmission);
                                            if (outcome.IsDenied) {
                                                return VoteResponse.Denied(outcome.ToString());
                                            }

                                            //if (!Voat.UserHelper.CanUserDownVoteInSubverse(submission.Subverses, userName)) {
                                            //    return VoteResponse.Denied("Subverse MinCPP requirement not met for downvote.");
                                            //}
                                            submission.Dislikes++;
                                        }
                                
                                        var t = new Votingtracker {
                                            MessageId = id,
                                            UserName = userName,
                                            VoteStatus = vote,
                                            Timestamp = DateTime.Now
                                        };
                                
                                        _db.Votingtrackers.Add(t);
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
                                            
                                            submission.Likes--;

                                            _db.Votingtrackers.Remove(votingSubmissionTracker);
                                        
                                            _db.SaveChanges();

                                        }

                                        response = response = VoteResponse.Success(0, REVOKE_MSG);

                                        break;
                                    case -1:
                                        //change upvote to downvote

                                        if (votingSubmissionTracker != null) {

                                            submission.Likes--;
                                            submission.Dislikes++;

                                            votingSubmissionTracker.VoteStatus = vote;
                                            votingSubmissionTracker.Timestamp = CurrentDate;
                                            
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
                                            submission.Dislikes--;
                                            _db.Votingtrackers.Remove(votingSubmissionTracker);
                                            _db.SaveChanges();
                                            response = VoteResponse.Success(0, REVOKE_MSG);
                                        }
                                        break;
                                    case 1:

                                        //change downvote to upvote
                                        var votingTracker = _db.Votingtrackers.FirstOrDefault(x => x.MessageId == id && x.UserName == userName);

                                        if (votingTracker != null) {
                                            submission.Likes++;
                                            submission.Dislikes--;

                                            votingTracker.VoteStatus = vote;
                                            votingTracker.Timestamp = CurrentDate; 
                                            
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


        public Message GetSubmission(int submissionID) {

            var processor = new Action<Message>(x => {
                    //x.MessageContent = (formatMessageContent ? Utils.Formatting.FormatMessage(x.MessageContent) : x.MessageContent);
                    x.Name = (x.Anonymized ? x.Id.ToString() : x.Name);
                });

            return GetSubmission(submissionID, new Func<Models.Message, Models.Message>(x => x), processor);
        }
  
        public T GetSubmission<T>(int submissionID, Func<Message, T> selector, Action<T> processor) {

            var query = (from x in _db.Messages
                              where x.Id == submissionID
                              select x);

            var record = query.Select(selector).FirstOrDefault();

            Process(record, processor);

            return record;

        }
        public List<ApiSubscription> GetSubscriptions(string userName) {


            var subs = (from x in _db.Subscriptions
                         where x.Username == userName
                         select new ApiSubscription() { Name = x.SubverseName, Type = SubscriptionType.Subverse }).ToList();

            var sets = (from x in _db.Usersetsubscriptions
                        where x.Username == userName
                        select new ApiSubscription() { Name = x.Userset.Name, Type = SubscriptionType.Set }).ToList();

            subs.AddRange(sets);

            return subs;

        }
       
        public List<Message> GetUserSubmissions(string subverse, string userName, SearchOptions options) {

            var selector = new Func<Message, Message>(x => x);
            var processor = new Action<Message>(x => x.Name = (x.Anonymized ? x.Id.ToString() : x.Name));

            return GetUserSubmissions(subverse, userName, options, selector, processor);

        }
        public List<T> GetUserSubmissions<T>(string subverse, string userName, SearchOptions options, Func<Message, T> selector, Action<T> processor) {
            
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

            IQueryable<Message> query;

            subverse = ToCorrectSubverseCasing(subverse);

            query = (from x in _db.Messages
                     where (x.Name == userName && !x.Anonymized)
                     && (x.Subverse == subverse || subverse == null)   
                     select x);

            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = query.Select(selector).ToList();

            Process(results, processor);

            return results;

        }
        
        public List<Message> GetSubmissions(string subverse, SearchOptions options) {

            var selector = new Func<Message, Message>(x => x);
            var processor = new Action<Message>(x => x.Name = (x.Anonymized ? x.Id.ToString() : x.Name));

            return GetSubmissions(subverse, options, selector, processor);

        }

        public List<T> GetSubmissions<T>(string subverse, SearchOptions options, Func<Message, T> selector, Action<T> processor) {

            
            //System.Web.HttpContext.Current.Cache.Add("voat", o);


            if (String.IsNullOrEmpty(subverse)) {
                throw new VoatValidationException("A subverse must be provided.");
            }

            if (options == null) {
                options = new SearchOptions();
            }
            

            IQueryable<Message> query;

            switch (subverse.ToLower()){

                //for *special* subverses, this is UNDONE
                case "_front":
                    if (User.Identity.IsAuthenticated && UserHelper.SubscriptionCount(User.Identity.Name) > 0) {
                        query = (from x in _db.Messages
                                 join subscribed in _db.Subscriptions on x.Subverse equals subscribed.SubverseName
                                 where subscribed.Username == User.Identity.Name 
                                 select x);
                    } else {
                        //if no user, default to default
                        query = (from x in _db.Messages
                                join defaults in _db.Defaultsubverses on x.Subverse equals defaults.name
                                select x);
                    }
                    break;

                case "_default":

                    query = (from x in _db.Messages
                            join defaults in _db.Defaultsubverses on x.Subverse equals defaults.name
                            select x);
                    break;
                case "_any":

                    query = (from x in _db.Messages.Include("Subverses")
                             where !x.Subverses.forced_private && !x.Subverses.private_subverse
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

                    query = (from x in _db.Messages
                             where  x.Subverses.minimumdownvoteccp == 0 
                                    && (!x.Subverses.forced_private && !x.Subverses.private_subverse)
                                    && (x.Subverses.rated_adult && nsfw || !x.Subverses.rated_adult)
                             select x);

                    break;
                //for regular subverse queries
                default:

                    if (!SubverseExists(subverse)) {
                        throw new VoatNotFoundException("Subverse '{0}' not found.", subverse);
                    }

                    subverse = ToCorrectSubverseCasing(subverse);

                    query = (from x in _db.Messages
                             where (x.Subverse == subverse || subverse == null)
                             select x);
                    break;
            }

            query = query.Where(x => x.Name != "deleted");

            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = query.Select(selector).ToList();

            Process(results, processor);

            return results;
        }

        public Subverse GetSubverseInfo(string sub) {

            var submission = (from x in _db.Subverses
                              where x.name == sub
                              select x).FirstOrDefault();

            if (submission != null) {
                submission.sidebar = Utilities.Formatting.FormatMessage(submission.sidebar, true);
            }

            return submission;
        }
        
        //public List<T> GetSubmissions<T>(string subverse, SearchOptions options, Func<Message, T> selector, Action<T> processor) {

        public T GetSubverseInfo<T>(string sub, Func<Subverse, T> selector, Action<T> processor) {

            var submission = (from x in _db.Subverses
                              where x.name == sub
                              select x).FirstOrDefault();

          

            if (submission != null) {
                submission.sidebar = Utilities.Formatting.FormatMessage(submission.sidebar, true);
            }

            return selector.Invoke(submission);
        }


        
        public bool SubverseExists(string subverse) {

            return _db.Subverses.Any(x => x.name == subverse);
        
        }
        public string ToCorrectSubverseCasing(string subverse) {
            if (!String.IsNullOrEmpty(subverse)) {
                var sub = _db.Subverses.FirstOrDefault(x => x.name == subverse);
                return (sub == null ? null : sub.name);
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
        public Message PostSubmission(string subverse, UserSubmission submission) {


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

            Message m = new Message();
            m.Name = User.Identity.Name;
            m.Date = CurrentDate;
            m.Subverse = ToCorrectSubverseCasing(subverse); // < Needs to be the same casing as EF fails if relational keys aren't cased identically (Thanks @Siege!)

            //1: Self Post, 2: Link Post
            m.Type = (String.IsNullOrEmpty(submission.Url) ? 1 : 2);


            if (m.Type == 1) {

                m.Title = submission.Title;
                m.MessageContent = submission.Content;
                m.Linkdescription = null;

            } else {

                m.Title = null;
                m.MessageContent = submission.Url;
                m.Linkdescription = submission.Title;

            }

            _db.Messages.Add(m);

            _db.SaveChanges();

            return m;

        }
        [Authorize]
        public Message UpdateSubmission(int submissionID, UserSubmission submission) {

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
                
            if (m.Name != User.Identity.Name){
                throw new VoatSecurityException(String.Format("Submission can not be edited by account"));
            }


            

            //only allow edits for self posts
            if (m.Type == 1) {
                m.MessageContent = submission.Content ?? m.MessageContent;
            }

            //allow edit of title if in 10 minute window
            if (CurrentDate.Subtract(m.Date).TotalMinutes <= 10.0f) {

                if (!String.IsNullOrEmpty(submission.Title) && Utilities.Submissions.ContainsUnicode(submission.Title)) {
                    throw new VoatValidationException("Submission title can not contain Unicode characters.");
                }

                if (m.Type == 1) {
                    m.Title = (String.IsNullOrEmpty(submission.Title) ? m.Title : submission.Title);
                } else {
                    m.Linkdescription = (String.IsNullOrEmpty(submission.Title) ? m.Linkdescription : submission.Title);
                }
            }

            m.LastEditDate = CurrentDate;

            _db.SaveChanges();

            return m;

        }

        [Authorize]
        //LOGIC COPIED FROM SubmissionController.DeleteSubmission(int)
        public void DeleteSubmission(int submissionID) {

             var submissionToDelete = _db.Messages.Find(submissionID);

             if (submissionToDelete != null) {
                 // delete submission if delete request is issued by submission author
                 if (submissionToDelete.Name == User.Identity.Name) {
                     submissionToDelete.Name = "deleted";
                     submissionToDelete.IsDeleted = true;

                     if (submissionToDelete.Type == 1) {
                         submissionToDelete.MessageContent = "deleted by author at " + DateTime.Now;
                     } else {
                         submissionToDelete.MessageContent = "http://voat.co";
                     }

                     // remove sticky if submission was stickied
                     var existingSticky = _db.Stickiedsubmissions.FirstOrDefault(s => s.Submission_id == submissionID);
                     if (existingSticky != null) {
                         _db.Stickiedsubmissions.Remove(existingSticky);
                     }

                     _db.SaveChanges();
                 }

                 // delete submission if delete request is issued by subverse moderator
                 else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse)) {
                     // mark submission as deleted (TODO: don't use name, add a new bit field to messages table instead)
                     submissionToDelete.Name = "deleted";
                     submissionToDelete.IsDeleted = true;

                     // move the submission to removal log
                     var removalLog = new SubmissionRemovalLog {
                         SubmissionId = submissionToDelete.Id,
                         Moderator = User.Identity.Name,
                         ReasonForRemoval = "This feature is not yet implemented",
                         RemovalTimestamp = DateTime.Now
                     };

                     _db.SubmissionRemovalLogs.Add(removalLog);

                    // notify submission author that his submission has been deleted by a moderator
                    var message =
                        "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.Id + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " at " + DateTime.Now + "  " + Environment.NewLine +
                        "Original submission content was: " + Environment.NewLine + "---" + Environment.NewLine +
                        (submissionToDelete.Type == 1 ?
                            "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                            "Submission content: " + submissionToDelete.MessageContent
                        :
                            "Link description: " + submissionToDelete.Linkdescription + ", " + Environment.NewLine +
                            "Link URL: " + submissionToDelete.MessageContent
                        );

                     MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            submissionToDelete.Name,
                            "Your submission has been deleted by a moderator",
                            message
                     );


                     // remove sticky if submission was stickied
                     var existingSticky = _db.Stickiedsubmissions.FirstOrDefault(s => s.Submission_id == submissionID);
                     if (existingSticky != null) {
                         _db.Stickiedsubmissions.Remove(existingSticky);
                     }

                     _db.SaveChanges();
                 } else {
                     throw new VoatSecurityException("User doesn't have permission to delete submission.");
                 }
             }
        }
        
        public List<Models.Comment> GetComments(int submissionID, SearchOptions options) {
            return GetComments(submissionID, options, new Func<Models.Comment, Models.Comment>(x => x), new Action<Models.Comment>(x => x.Name = (x.Anonymized ? x.Id.ToString() : x.Name)));
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
                            (x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
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
                            (!x.Message.Subverses.private_subverse && submissionID == null) && //got to filter out comments from private subs if calling from the streaming endpoint
                            (x.MessageId == submissionID || submissionID == null)
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

            var direct = _db.Comments.Where(x => x.Id == commentID);
            
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
                var commentSubverse = comment.Message.Subverse;
                // delete comment if the comment author is currently logged in user
                if (comment.Name == User.Identity.Name) {
                    comment.CommentContent = "deleted by author at " + CurrentDate.ToLongDateString();
                    comment.Name = "deleted";
                    comment.IsDeleted = true;
                }// delete comment if delete request is issued by subverse moderator
                else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, commentSubverse)) {
                    comment.Name = "deleted";
                    comment.IsDeleted = true;
                    comment.CommentContent = "deleted by moderator at " + CurrentDate;

                    // notify comment author that his comment has been deleted by a moderator
                    MesssagingUtility.SendPrivateMessage(
                        "Voat",
                        comment.Name,
                        "Your comment has been deleted by a moderator",
                        "Your [comment](/v/" + commentSubverse + "/comments/" + comment.MessageId + "/" + comment.Id + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " on: " + CurrentDate + "  " + Environment.NewLine +
                        "Original comment content was: " + Environment.NewLine +
                        "---" + Environment.NewLine +
                        comment.CommentContent
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

                if (current.Name.Trim() == User.Identity.Name) {

                    current.LastEditDate = CurrentDate;

                    var escapedCommentContent = WebUtility.HtmlEncode(comment);
                    current.CommentContent = escapedCommentContent;

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave)) {
                        current.CommentContent = ContentProcessor.Instance.Process(current.CommentContent, ProcessingStage.InboundPreSave, current);
                    }

                    _db.SaveChanges();

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave)) {
                        ContentProcessor.Instance.Process(current.CommentContent, ProcessingStage.InboundPostSave, current);
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
            var submissionid = c.Message.Id;
            return PostComment(submissionid, parentCommentID, comment);

        }
        [Authorize]
        public Comment PostComment(int submissionID, int? parentCommentID, string comment) {

            var submission = _db.Messages.Find(submissionID);

            if (submission == null) {
                throw new VoatNotFoundException("submissionID", submissionID, "Can not find submission");
            }

            var subverse = _db.Subverses.Where(x => x.name == submission.Subverse).FirstOrDefault();
           
            var c = new Comment();
            c.Date = DateTime.Now;
            c.Name = User.Identity.Name;
            c.CommentContent = comment;
            c.ParentId = (parentCommentID > 0 ? parentCommentID : (int?)null);
            c.MessageId = submissionID;
            c.Votes = 0;
            c.Likes = 0;
            c.Anonymized = (submission.Anonymized || subverse.anonymized_mode);
            

            // check if author is banned, don't save the comment or send notifications if true
            if (!UserHelper.IsUserGloballyBanned(User.Identity.Name) && !UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse)) {
                _db.Comments.Add(c);

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave)) {
                    c.CommentContent = ContentProcessor.Instance.Process(c.CommentContent, ProcessingStage.InboundPreSave, c);
                }

                _db.SaveChangesAsync();

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave)) {
                    ContentProcessor.Instance.Process(c.CommentContent, ProcessingStage.InboundPostSave, c);
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


            var userBadges = (from x in _db.Userbadges
                              join b in _db.Badges on x.BadgeId equals b.BadgeId
                              where x.Username == userName
                              select new ApiUserBadge() {
                                  Awarded = x.Awarded,
                                  BadgeName = b.BadgeName,
                                  BadgeTitle = b.BadgeTitle,
                                  BadgeGraphics = b.BadgeGraphics,
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
        public Userpreference GetUserPreferences(string userName) {

            var selector = new Func<Userpreference, Userpreference>(x => x);

            return GetUserPreferences(userName, selector, null);

        }

        public T GetUserPreferences<T>(string userName, Func<Models.Userpreference, T> selector, Action<T> processor) {

            var query = _db.Userpreferences.Where(x => (x.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)));

            var results = query.Select(selector).FirstOrDefault();

            Process(results, processor);

            return results;
                    
        }

        private static IQueryable<Message> ApplySubmissionSearch(SearchOptions options, IQueryable<Message> query) {

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
                    query = query.Where(m => m.Title.Contains(x) || m.MessageContent.Contains(x) || m.Linkdescription.Contains(x));
                });
                
            }
            if (options.StartDate.HasValue) {
                query = query.Where(x => x.Date >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue) {
                query = query.Where(x => x.Date <= options.EndDate.Value);
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
                        query = query.OrderBy(x => x.Date);
                    } else {
                        query = query.OrderByDescending(x => x.Date);
                    }
                    break;

                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Likes); 
                    } else {
                        query = query.OrderByDescending(x => x.Likes);
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
                        query = query.OrderBy(x => x.Comments.OrderBy(c => c.Date).FirstOrDefault().Date);
                    } else {
                        query = query.OrderByDescending(x => x.Comments.OrderBy(c => c.Date).FirstOrDefault().Date);
                    }
                    break;
                case SortAlgorithm.Bottom:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Dislikes);
                    } else {
                        query = query.OrderByDescending(x => x.Dislikes);
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
                    query = query.Where(m => m.CommentContent.Contains(x));
                });

            }
            if (options.StartDate.HasValue) {
                query = query.Where(x => x.Date >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue) {
                query = query.Where(x => x.Date <= options.EndDate.Value);
            }
            //TODO: Implement Depth in Comment Table
            //if (options.Depth > 0) {
            //    query = query.Where(x => 1 == 1);
            //}

            switch (options.Sort) {
                case SortAlgorithm.Hot:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => (x.Likes - x.Dislikes));
                    } else {
                        query = query.OrderByDescending(x => (x.Likes - x.Dislikes));
                    }
                    break;
                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Likes);
                    } else {
                        query = query.OrderByDescending(x => x.Dislikes);
                    }
                    break;
                case SortAlgorithm.New:
                    if (options.SortDirection == SortDirection.Reverse) {
                        query = query.OrderBy(x => x.Date);
                    } else {
                        query = query.OrderByDescending(x => x.Date);
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
                    
                    var c = _db.Commentsavingtrackers.FirstOrDefault(x => x.CommentId == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    
                    if (c == null && (forceAction == null || forceAction.HasValue && forceAction.Value)) {

                        c = new Commentsavingtracker() { CommentId = ID, UserName = currentUserName, Timestamp = CurrentDate };
                        _db.Commentsavingtrackers.Add(c);
                        isSaved = true;

                    } else if (c != null && (forceAction == null || forceAction.HasValue && !forceAction.Value)) {
                        
                        _db.Commentsavingtrackers.Remove(c);
                        isSaved = false;
                   
                    }
                    _db.SaveChanges();

                    break;
                case ContentType.Submission:
                    
                    var s = _db.Savingtrackers.FirstOrDefault(x => x.MessageId == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    if (s == null && (forceAction == null || forceAction.HasValue && forceAction.Value)) {
                        
                        s = new Savingtracker() { MessageId = ID, UserName = currentUserName, Timestamp = CurrentDate };
                        _db.Savingtrackers.Add(s);
                        isSaved = true;

                    } else if (s != null && (forceAction == null || forceAction.HasValue && !forceAction.Value)) {

                        _db.Savingtrackers.Remove(s);
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

            SaveUserPrefernces(new Userpreference() {
                //Avatar = preferences.Avatar,
                //Shortbio = preferences.Bio,
                Clicking_mode = preferences.ClickingMode,
                Disable_custom_css = preferences.DisableCustomCSS,
                Enable_adult_content = preferences.EnableAdultContent,
                Language = preferences.Language,
                Night_mode = preferences.EnableNightMode,
                Public_subscriptions = preferences.PubliclyShowSubscriptions,
                Public_votes = preferences.PubliclyDisplayVotes
            });

        }

        [Authorize]
        public void SaveUserPrefernces(Userpreference preferences) {
            var p = (from x in _db.Userpreferences
                     where x.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();

            if (p == null) {
                p = new Userpreference();
                p.Username = User.Identity.Name;
            }

            if (!String.IsNullOrEmpty(preferences.Avatar)) {
                p.Avatar = preferences.Avatar;
            }
            if (!String.IsNullOrEmpty(preferences.Shortbio)) {
                p.Shortbio = preferences.Shortbio;
            }
            if (!String.IsNullOrEmpty(preferences.Language)) {
                p.Language = preferences.Language;
            }
            p.Clicking_mode = preferences.Clicking_mode;
            p.Disable_custom_css = preferences.Disable_custom_css;
            p.Enable_adult_content = preferences.Enable_adult_content;
            p.Night_mode = preferences.Night_mode;
            p.Public_subscriptions = preferences.Public_subscriptions;
            p.Public_votes = preferences.Public_votes;
            p.Topmenu_from_subscriptions = preferences.Topmenu_from_subscriptions;

            _db.Userpreferences.Add(p);

            _db.SaveChanges();
            
            

        }

        [Authorize]
        //[ApiModelStateValidation]
        public void SendMessageReply(int id, string value) {

            //throw new NotImplementedException();

            //THIS WILL BE WAY EASIER TO IMPLEMENT AFTER THE MESSAGE TABLES ARE MERGED.
            //find message
            var c = (from x in _db.Commentreplynotifications
                     where x.Id == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (c != null) {
                PostCommentReply(c.CommentId, value);
                return;
            }

            var sub = (from x in _db.Postreplynotifications
                       where x.Id == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();
            if (sub != null) {
                PostComment(sub.SubmissionId, sub.CommentId, value);
                return;
            }
            var pm = (from x in _db.Privatemessages
                      where x.Id == id && x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
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



                Privatemessage msg = new Privatemessage();

                // send the message
                msg.Recipient = sendUserMessage.Recipient;
                msg.Body = sendUserMessage.Message;
                msg.Subject = sendUserMessage.Subject;
                msg.Timestamp = CurrentDate;
                msg.Sender = User.Identity.Name;
                msg.Status = true;

                _db.Privatemessages.Add(msg);

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
                var msgs = (from x in _db.Privatemessages
                            where (
                                ((x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type | MessageType.Inbox) > 0) ||
                                (x.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && (type | MessageType.Sent) > 0)) && 
                                x.Status == (state == MessageState.Unread))
                            select new UserMessage() {
                                ID = x.Id,
                                CommentID = null,
                                SubmissionID = null,
                                Subverse = null,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = x.Subject,
                                Content = x.Body,
                                Unread = x.Status,
                                Type = (x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) ? MessageType.Inbox : MessageType.Sent),
                                SentDate = x.Timestamp
                            });
                messages.AddRange(msgs.ToList());
            }
            //Comment Replies, Mention Replies
            if ((type & MessageType.Comment) > 0 || (type & MessageType.Mention) > 0) {
                var msgs = (from x in _db.Commentreplynotifications
                            join s in _db.Messages on x.SubmissionId equals s.Id
                            join c in _db.Comments on x.CommentId equals c.Id into commentJoin
                            from comment in commentJoin.DefaultIfEmpty()
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                                && x.Status == (state == MessageState.Unread)
                                )
                            select new UserMessage() {
                                ID = x.Id,
                                CommentID = (x.CommentId > 0 ? x.CommentId : (int?)null),
                                SubmissionID = x.SubmissionId,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = s.Title,
                                Content = (comment == null ? s.MessageContent : comment.CommentContent),
                                Unread = x.Status,
                                Type = (s.MessageContent.Contains(User.Identity.Name) ? MessageType.Mention : MessageType.Comment), //TODO: Need to determine if comment reply or mention
                                SentDate = x.Timestamp
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
                var msgs = (from x in _db.Postreplynotifications
                            join c in _db.Comments on x.CommentId equals c.Id
                            join s in _db.Messages on c.MessageId equals s.Id
                            where (
                                x.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) &&
                                x.Status == (state == MessageState.Unread))
                            select new UserMessage() {
                                ID = x.Id,
                                CommentID = x.CommentId,
                                SubmissionID = x.SubmissionId,
                                Subverse = x.Subverse,
                                Recipient = x.Recipient,
                                Sender = x.Sender,
                                Subject = (s.Type == 1 ? s.Linkdescription : s.Title),
                                Content = c.CommentContent,
                                Unread = x.Status,
                                Type = MessageType.Submission,
                                SentDate = x.Timestamp
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
                                    var m = _db.Commentreplynotifications.First(x => x.Id == msg.ID);
                                    m.Status = false;
                                    break;
                                case MessageType.Inbox:
                                    var p = _db.Privatemessages.First(x => x.Id == msg.ID);
                                    p.Status = false;

                                    break;
                                case MessageType.Submission:
                                    var s = _db.Postreplynotifications.First(x => x.Id == msg.ID);
                                    s.Status = false;
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


            var result = (from x in _db.Votingtrackers
                          where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                          && ((compareDate.HasValue && x.Timestamp >= compareDate) || !compareDate.HasValue)
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

            var result = (from x in _db.Commentvotingtrackers
                          where x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                           && ((compareDate.HasValue && x.Timestamp >= compareDate) || !compareDate.HasValue)
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
                            x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                            && (x.Message.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.Date >= compareDate)
                          select x).Count();
            return result;
        }

        public int UserSubmissionCount(string userName, TimeSpan? span, SubmissionType? type = null, string subverse = null) {

            DateTime? compareDate = null;
            if (span.HasValue) {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.Messages
                          where
                            x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                            && (x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                            && (compareDate.HasValue && x.Date >= compareDate)
                            && (type != null && x.Type == (int)type.Value) || type == null
                          select x).Count();
            return result;
        }

        public Score UserContributionPoints(string userName, ContentType type, string subverse = null)
        {

            Score s = new Score();

            if ((type & ContentType.Comment) > 0) {
                var totals = (from x in _db.Comments.Include("Message")
                              where x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Message.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.Name into y
                              select new {
                                  up = y.Sum(ups => ups.Likes),
                                  down = y.Sum(downs => downs.Dislikes)
                              }).FirstOrDefault();

                if (totals != null) {
                    s.Combine(new Score() { UpCount = totals.up, DownCount = totals.down });
                }
            }

            if ((type & ContentType.Submission) > 0) {
                var totals = (from x in _db.Messages
                              where x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                 && (x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
                              group x by x.Name into y
                              select new {
                                  up = y.Sum(ups => ups.Likes),
                                  down = y.Sum(downs => downs.Dislikes)
                              }).FirstOrDefault();
                if (totals != null) {
                    s.Combine(new Score() { UpCount = totals.up, DownCount = totals.down });
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