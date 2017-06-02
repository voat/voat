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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
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
using Voat.Domain;
using Voat.Caching;
using Dapper;
using Voat.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Voat.Data
{
    public partial class Repository : IDisposable
    {
        private static LockStore _lockStore = new LockStore();
        private VoatDataContext _db;

        #region Class
        public Repository() : this(new VoatDataContext())
        {
            /*no-op*/
        }

        public Repository(VoatDataContext dataContext)
        {
            _db = dataContext;

            //Prevent EF from creating dynamic proxies, those mother fathers. This killed
            //us during The Fattening, so we throw now -> (╯°□°)╯︵ ┻━┻
            //CORE_PORT: Prop not available
            //_db.Configuration.ProxyCreationEnabled = false;
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
        #endregion  

        #region Vote
        [Authorize]
        public VoteResponse VoteComment(int commentID, int vote, string addressHash, bool revokeOnRevote = true)
        {
            DemandAuthentication();

            //make sure we don't have bad int values for vote
            if (Math.Abs(vote) > 1)
            {
                throw new ArgumentOutOfRangeException("vote", "Valid values for vote are only: -1, 0, 1");
            }

            string userName = UserIdentity.UserName;
            var ruleContext = new VoatRuleContext();
            ruleContext.PropertyBag.AddressHash = addressHash;
            RuleOutcome outcome = null;

            string REVOKE_MSG = "Vote has been revoked";

            var synclock_comment = _lockStore.GetLockObject(String.Format("comment:{0}", commentID));
            lock (synclock_comment)
            {
                var comment = _db.Comment.FirstOrDefault(x => x.ID == commentID);

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
                    var existingVoteTracker = _db.CommentVoteTracker.FirstOrDefault(x => x.CommentID == commentID && x.UserName == userName);
                    if (existingVoteTracker != null)
                    {
                        existingVote = existingVoteTracker.VoteStatus;
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
                                        VoteValue = GetVoteValue(userName, comment.UserName, ContentType.Comment, comment.ID, vote), //TODO: Need to set this to zero for Anon, MinCCP subs, and Private subs
                                        IPAddress = addressHash,
                                        CreationDate = Repository.CurrentDate
                                    };

                                    _db.CommentVoteTracker.Add(newVotingTracker);
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

                                        _db.CommentVoteTracker.Remove(existingVoteTracker);
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
                                        existingVoteTracker.VoteValue = GetVoteValue(userName, comment.UserName, ContentType.Comment, comment.ID, vote);
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
                                        _db.CommentVoteTracker.Remove(existingVoteTracker);
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
                                        existingVoteTracker.VoteValue = GetVoteValue(userName, comment.UserName, ContentType.Comment, comment.ID, vote);
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

            string userName = UserIdentity.UserName;
            var ruleContext = new VoatRuleContext();
            ruleContext.PropertyBag.AddressHash = addressHash;
            RuleOutcome outcome = null;

            string REVOKE_MSG = "Vote has been revoked";
            Data.Models.Submission submission = null;
            var synclock_submission = _lockStore.GetLockObject(String.Format("submission:{0}", submissionID));
            lock (synclock_submission)
            {
                submission = _db.Submission.FirstOrDefault(x => x.ID == submissionID);

                if (submission != null)
                {
                    if (submission.IsDeleted)
                    {
                        return VoteResponse.Ignored(0, "Deleted submissions cannot be voted");
                        //return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }
                    //if (submission.IsArchived)
                    //{
                    //    return VoteResponse.Ignored(0, "Archived submissions cannot be voted");
                    //    //return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    //}

                    //ignore votes if user owns it
                    if (String.Equals(submission.UserName, userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return VoteResponse.Ignored(0, "User is prevented from voting on own content");
                    }

                    //check existing vote
                    int existingVote = 0;
                    var existingVoteTracker = _db.SubmissionVoteTracker.FirstOrDefault(x => x.SubmissionID == submissionID && x.UserName == userName);
                    if (existingVoteTracker != null)
                    {
                        existingVote = existingVoteTracker.VoteStatus;
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
                                        VoteValue = GetVoteValue(userName, submission.UserName, ContentType.Submission, submission.ID, vote), //TODO: Need to set this to zero for Anon, MinCCP subs, and Private subs
                                        IPAddress = addressHash,
                                        CreationDate = Repository.CurrentDate
                                    };

                                    _db.SubmissionVoteTracker.Add(t);
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

                                        _db.SubmissionVoteTracker.Remove(existingVoteTracker);
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
                                        existingVoteTracker.VoteValue = GetVoteValue(userName, submission.UserName, ContentType.Submission, submission.ID, vote);
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

                                        _db.SubmissionVoteTracker.Remove(existingVoteTracker);
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
                                        existingVoteTracker.VoteValue = GetVoteValue(userName, submission.UserName, ContentType.Submission, submission.ID, vote);
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

        private int GetVoteValue(Subverse subverse, Data.Models.Submission submission, Vote voteStatus)
        {
            if (subverse.IsPrivate || subverse.MinCCPForDownvote > 0 || submission.IsAnonymized)
            {
                return 0;
            }
            return (int)voteStatus;
        }
        private int GetVoteValue(string sourceUser, string targetUser, ContentType contentType, int id, int voteStatus)
        {
            var q = new DapperQuery();
            q.Select = $"sub.\"IsPrivate\", s.\"IsAnonymized\", sub.\"MinCCPForDownvote\" FROM {SqlFormatter.Table("Subverse", "sub", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON s.\"Subverse\" = sub.\"Name\"";

            switch (contentType)
            {
                case ContentType.Comment:
                    q.Select += $" INNER JOIN {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} ON c.\"SubmissionID\" = s.\"ID\"";
                    q.Where = "c.\"ID\" = @ID";
                    break;
                case ContentType.Submission:
                    q.Where = "s.\"ID\" = @ID";
                    break;
            }

            var record = _db.Connection.QueryFirst(q.ToString(), new { ID = id });

            if (record.IsPrivate || record.IsAnonymized || record.MinCCPForDownvote > 0)
            {
                return 0;
            }
            else
            {
                return voteStatus;
            }
        }
        #endregion Vote

        #region Subverse

        public IEnumerable<SubverseInformation> GetDefaultSubverses()
        {
            var defaults = (from d in _db.DefaultSubverse
                            join x in _db.Subverse on d.Subverse equals x.Name
                            orderby d.Order
                            select new SubverseInformation
                            {
                                Name = x.Name,
                                SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                                CreationDate = x.CreationDate,
                                Description = x.Description,
                                IsAdult = x.IsAdult,
                                Title = x.Title,
                                //Type = x.Type,
                                Sidebar = x.SideBar
                            }).ToList();
            return defaults;
        }

        public IEnumerable<SubverseInformation> GetTopSubscribedSubverses(int count = 200)
        {
            var subs = (from x in _db.Subverse
                        orderby x.SubscriberCount descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            IsAdult = x.IsAdult,
                            Title = x.Title,
                            //Type = x.Type,
                            Sidebar = x.SideBar
                        }).Take(count).ToList();
            return subs;
        }

        public IEnumerable<SubverseInformation> GetNewestSubverses(int count = 100)
        {
            var subs = (from x in _db.Subverse
                        orderby x.CreationDate descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            IsAdult = x.IsAdult,
                            Title = x.Title,
                            //Type = x.Type,
                            Sidebar = x.SideBar
                        }
                        ).Take(count).ToList();
            return subs;
        }

        public IEnumerable<SubverseInformation> FindSubverses(string phrase, int count = 50)
        {
            var subs = (from x in _db.Subverse
                        where x.Name.Contains(phrase) || x.Description.Contains(phrase)
                        orderby x.SubscriberCount descending
                        select new SubverseInformation
                        {
                            Name = x.Name,
                            SubscriberCount = x.SubscriberCount.HasValue ? x.SubscriberCount.Value : 0,
                            CreationDate = x.CreationDate,
                            Description = x.Description,
                            IsAdult = x.IsAdult,
                            Title = x.Title,
                            //Type = x.Type,
                            Sidebar = x.SideBar
                        }
                        ).Take(count).ToList();
            return subs;
        }

        public Subverse GetSubverseInfo(string subverse, bool filterDisabled = false)
        {
            using (var db = new VoatDataContext())
            {
                db.EnableCacheableOutput();
                var query = (from x in db.Subverse
                             where x.Name == subverse
                             select x);
                if (filterDisabled)
                {
                    query = query.Where(x => x.IsAdminDisabled != true);
                }
                var submission = query.FirstOrDefault();
                return submission;
            }
        }

        public string GetSubverseStylesheet(string subverse)
        {
            var sheet = (from x in _db.Subverse
                         where x.Name.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                         select x.Stylesheet).FirstOrDefault();
            return String.IsNullOrEmpty(sheet) ? "" : sheet;
        }

        public IEnumerable<Data.Models.SubverseModerator> GetSubverseModerators(string subverse)
        {
            var data = (from x in _db.SubverseModerator
                        where x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        orderby x.Power ascending, x.CreationDate ascending
                        select x).ToList();

            return data.AsEnumerable();
        }

        public IEnumerable<Data.Models.SubverseModerator> GetSubversesUserModerates(string userName)
        {
            var data = (from x in _db.SubverseModerator
                        where x.UserName == userName
                        select x).ToList();

            return data.AsEnumerable();
        }

        public async Task<CommandResponse> CreateSubverse(string name, string title, string description, string sidebar = null)
        {
            DemandAuthentication();

            //Evaulate Rules
            VoatRuleContext context = new VoatRuleContext();
            context.PropertyBag.SubverseName = name;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.CreateSubverse);
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<object>(outcome, null);
            }

            try
            {
                // setup default values and create the subverse
                var subverse = new Subverse
                {
                    Name = name,
                    Title = title,
                    Description = description,
                    SideBar = sidebar,
                    CreationDate = Repository.CurrentDate,
                    //Type = "link",
                    IsThumbnailEnabled = true,
                    IsAdult = false,
                    IsPrivate = false,
                    MinCCPForDownvote = 0,
                    IsAdminDisabled = false,
                    CreatedBy = UserIdentity.UserName,
                    SubscriberCount = 0
                };

                _db.Subverse.Add(subverse);
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                await SubscribeUser(new DomainReference(DomainType.Subverse, subverse.Name), SubscriptionAction.Subscribe).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);


                // register user as the owner of the newly created subverse
                var tmpSubverseAdmin = new Models.SubverseModerator
                {
                    Subverse = name,
                    UserName = UserIdentity.UserName,
                    Power = 1
                };
                _db.SubverseModerator.Add(tmpSubverseAdmin);
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);


                // go to newly created Subverse
                return CommandResponse.Successful();
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<CommandResponse>(ex);
            }
        }
        public async Task<Domain.Models.Submission> GetSticky(string subverse)
        {
            var x = await _db.StickiedSubmission.FirstOrDefaultAsync(s => s.Subverse == subverse).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            if (x != null)
            {
                var submission = GetSubmission(x.SubmissionID);
                if (!submission.IsDeleted)
                {
                    return submission.Map();
                }
            }
            return null;
        }
        #endregion Subverse

        #region Submissions

        public int GetCommentCount(int submissionID)
        {
            using (VoatDataContext db = new VoatDataContext())
            {
                var cmd = db.Connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} WHERE c.\"SubmissionID\" = @SubmissionID AND c.\"IsDeleted\" != {SqlFormatter.BooleanLiteral(true)}";
                var param = cmd.CreateParameter();
                param.ParameterName = "SubmissionID";
                param.DbType = System.Data.DbType.Int32;
                param.Value = submissionID;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result); //PostgreSQL Fix for invalid cast. No idea why.
            }
        }

        public IEnumerable<Data.Models.Submission> GetTopViewedSubmissions()
        {
            var startDate = CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var data = (from submission in _db.Submission
                        join subverse in _db.Subverse on submission.Subverse equals subverse.Name
                        where submission.ArchiveDate == null && !submission.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && submission.CreationDate >= startDate && submission.CreationDate <= CurrentDate
                        where !(from bu in _db.BannedUser select bu.UserName).Contains(submission.UserName)
                        where !subverse.IsAdminDisabled.Value

                        //where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(UserIdentity.UserName)
                        orderby submission.Views descending
                        select submission).Take(5).ToList();
            return data.AsEnumerable();
        }

        public string SubverseForSubmission(int submissionID)
        {
            var subname = (from x in _db.Submission
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
            var result = (from x in _db.Submission
                          where x.ID == submissionID
                          select x.UserName).FirstOrDefault();
            return result;
        }

        public Models.Submission FindSubverseLinkSubmission(string subverse, string url, TimeSpan cutOffTimeSpan)
        {
            var cutOffDate = CurrentDate.Subtract(cutOffTimeSpan);

            url = url.ToLower();
            subverse = subverse.ToLower();

            var submission =  _db.Submission.FirstOrDefault(s =>
                s.Url.ToLower() == url
                && s.Subverse.ToLower() == subverse
                && s.CreationDate > cutOffDate
                && s.IsDeleted == false);

            return submission;
        }

        public int FindUserLinkSubmissionCount(string userName, string url, TimeSpan cutOffTimeSpan)
        {
            var cutOffDate = CurrentDate.Subtract(cutOffTimeSpan);
            userName = userName.ToLower();
            url = url.ToLower();

            var count = _db.Submission.Count(s =>
                s.Url.ToLower() == url
                && s.UserName.ToLower() == userName
                && s.CreationDate > cutOffDate);

            return count; 
        }

        private Models.Submission GetSubmissionUnprotected(int submissionID)
        {
            var query = (from x in _db.Submission
                         where x.ID == submissionID
                         select x);

            var record = query.FirstOrDefault();
            return record;
        }

        public async Task<IEnumerable<Models.Submission>> GetUserSubmissions(string subverse, string userName, SearchOptions options)
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

            IQueryable<Models.Submission> query = null;

            subverse = ToCorrectSubverseCasing(subverse);
            // Postgre Port: Something funky was going on with the original expression
            //query = (from x in _db.Submissions
            //         where
            //            (x.UserName == userName && x.IsAnonymized == false && x.IsDeleted == false)
            //            && (x.Subverse == subverse || subverse == null)
            //         select x);

            if (String.IsNullOrEmpty(subverse))
            {
                query = (from x in _db.Submission
                         where
                            x.UserName == userName && x.IsAnonymized == false && x.IsDeleted == false
                         select x);
            }
            else
            {
                query = (from x in _db.Submission
                         where
                            x.UserName == userName && x.IsAnonymized == false && x.IsDeleted == false
                            && (x.Subverse == subverse)
                         select x);
            }
           
            query = ApplySubmissionSearch(options, query);

            //execute query
            var results = (await query.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT)).Select(Selectors.SecureSubmission);

            return results;
        }
        public async Task<IEnumerable<Data.Models.Submission>> GetSubmissionsByDomain(string domain, SearchOptions options)
        {
            var query = new DapperQuery();
            query.SelectColumns = "s.*";
            query.Select = $"SELECT DISTINCT {"{0}"} FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Subverse", "sub", null, "NOLOCK")} ON s.\"Subverse\" = sub.\"Name\"";
            query.Where = $"s.\"Type\" = {((int)SubmissionType.Link).ToString()} AND s.\"Url\" LIKE CONCAT('%', @Domain, '%')";
            ApplySubmissionSort(query, options);

            query.SkipCount = options.Index;
            query.TakeCount = options.Count;

            //Filter out all disabled subs
            query.Append(x => x.Where, $"sub.\"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)}");
            query.Append(x => x.Where, $"s.\"IsDeleted\" = {SqlFormatter.BooleanLiteral(false)}");

            query.Parameters = (new { Domain = domain }).ToDynamicParameters();

            //execute query
            var data = await _db.Connection.QueryAsync<Data.Models.Submission>(query.ToString(), query.Parameters);
            var results = data.Select(Selectors.SecureSubmission).ToList();
            return results;
        }
        public async Task<IEnumerable<Data.Models.Submission>> GetSubmissions(params int[] submissionID)
        {
            var query = new DapperQuery();
            query.SelectColumns = "s.*";
            query.Select = $"SELECT DISTINCT {"{0}"} FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")}";
            query.Where = $"s.\"ID\" {SqlFormatter.In("@IDs")}";

            query.Parameters = (new { IDs = submissionID }).ToDynamicParameters();

            //execute query
            var data = await _db.Connection.QueryAsync<Data.Models.Submission>(query.ToString(), query.Parameters);
            var results = data.Select(Selectors.SecureSubmission).ToList();
            return results;
        }

        public async Task<IEnumerable<Data.Models.Submission>> GetSubmissionsDapper(DomainReference domainReference, SearchOptions options)
        {
            //backwards compat with function body
            var type = domainReference.Type;
            var name = domainReference.Name;
            var ownerName = domainReference.OwnerName;

            if (!(type == DomainType.Subverse || type == DomainType.Set))
            {
                throw new NotImplementedException($"DomainType {type.ToString()} not implemented using this pipeline");
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new VoatValidationException("An object name must be provided.");
            }

            if (options == null)
            {
                options = new SearchOptions();
            }

            var query = new DapperQuery();
            query.SelectColumns = "s.*";
            query.Select = $"SELECT DISTINCT {"{0}"} FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} INNER JOIN  {SqlFormatter.Table("Subverse", "sub", null, "NOLOCK")} ON s.\"Subverse\" = sub.\"Name\"";

            //Parameter Declarations
            DateTime? startDate = options.StartDate;
            DateTime? endDate = options.EndDate;
            //string subverse = subverse;
            bool nsfw = false;
            string userName = null;

            UserData userData = null;
            if (UserIdentity.IsAuthenticated)
            {
                userData = new UserData(UserIdentity.UserName);
                userName = userData.UserName;
            }


            var joinSet = new Action<DapperQuery, string, string, SetType?, bool>((q, setName, setOwnerName, setType, include) =>
            {

                var set = GetSet(setName, setOwnerName, setType);
                if (set != null)
                {
                    var joinAlias = $"set{setName}";
                    var op = include ? "=" : "!=";
                    query.Append(x => x.Select, $"INNER JOIN {SqlFormatter.Table("SubverseSetList", joinAlias, null, "NOLOCK")} ON sub.\"ID\" {op} {joinAlias}.\"SubverseID\"");
                    query.Append(x => x.Where, $"{joinAlias}.\"SubverseSetID\" = @{joinAlias}ID");
                    query.Parameters.Add($"{joinAlias}ID", set.ID);
                }
            });

            switch (type)
            {
                case DomainType.Subverse:

                    bool filterBlockedSubverses = false;

                    switch (name.ToLower())
                    {
                        //Match Aggregate Subs
                        case AGGREGATE_SUBVERSE.FRONT:
                            joinSet(query, SetType.Front.ToString(), userName, SetType.Front, true);
                            //query.Append(x => x.Select, "INNER JOIN SubverseSubscription ss WITH (NOLOCK) ON s.Subverse = ss.Subverse");
                            query.Append(x => x.Where, $"s.\"ArchiveDate\" IS NULL AND s.\"IsDeleted\" = {SqlFormatter.BooleanLiteral(false)}");

                            //query = (from x in _db.Submissions
                            //         join subscribed in _db.SubverseSubscriptions on x.Subverse equals subscribed.Subverse
                            //         where subscribed.UserName == UserIdentity.UserName
                            //         select x);

                            break;
                        case AGGREGATE_SUBVERSE.DEFAULT:
                            //if no user or user has no subscriptions or logged in user requests default page
                            joinSet(query, "Default", null, null, true);
                            //query.Append(x => x.Select, "INNER JOIN DefaultSubverse ss WITH (NOLOCK) ON s.Subverse = ss.Subverse");

                            //sort default by relative rank on default if sorted by rank by default
                            options.Sort = (options.Sort == SortAlgorithm.Rank ? Domain.Models.SortAlgorithm.Relative : options.Sort);

                            if (Settings.IsVoatBranded && options.Sort == SortAlgorithm.Relative)
                            {
                                //This is a modification Voat uses in the default page
                                //Postgre Port
                                //query.Append(x => x.Where, "(s.\"UpCount\" - s.\"DownCount\" >= 20) AND ABS(DATEDIFF(HH, s.CreationDate, GETUTCDATE())) <= 24");
                                query.Append(x => x.Where, "(s.\"UpCount\" - s.\"DownCount\" >= 20) AND s.\"CreationDate\" >= @EndDate");
                                query.Parameters.Add("EndDate", CurrentDate.AddHours(-24));
                            }

                            //query = (from x in _db.Submissions
                            //         join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
                            //         select x);
                            break;
                        case AGGREGATE_SUBVERSE.ANY:
                            //allowing subverse marked private to not be filtered
                            //Should subs marked as private be excluded from an ANY query? I don't know.
                            //query.Where = "sub.IsAdminPrivate = 0 AND sub.IsPrivate = 0";
                            query.Where = $"sub.\"IsAdminPrivate\" = {SqlFormatter.BooleanLiteral(false)}";
                            //query = (from x in _db.Submissions
                            //         where
                            //         !x.Subverse1.IsAdminPrivate
                            //         && !x.Subverse1.IsPrivate
                            //         && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value)
                            //         select x);
                            break;

                        case AGGREGATE_SUBVERSE.ALL:
                        case "all":
                            filterBlockedSubverses = true;
                            ////Controller logic:
                            //IQueryable<Submission> submissionsFromAllSubversesByDate = 
                            //(from message in _db.Submissions
                            //join subverse in _db.Subverses on message.Subverse equals subverse.Name
                            //where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote == 0
                            //where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                            //where !subverse.IsAdminDisabled.Value
                            //where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(userName)
                            //select message).OrderByDescending(s => s.CreationDate);

                            nsfw = (UserIdentity.IsAuthenticated ? userData.Preferences.EnableAdultContent : false);

                            //v/all has certain conditions
                            //1. Only subs that have a MinCCP of zero
                            //2. Don't show private subs
                            //3. Don't show NSFW subs if nsfw isn't enabled in profile, if they are logged in
                            //4. Don't show blocked subs if logged in // not implemented
                            query.Where = $"sub.\"MinCCPForDownvote\" = 0 AND sub.\"IsAdminPrivate\" = {SqlFormatter.BooleanLiteral(false)} AND sub.\"IsPrivate\" = {SqlFormatter.BooleanLiteral(false)}";
                            if (!nsfw)
                            {
                                query.Where += $" AND sub.\"IsAdult\" = {SqlFormatter.BooleanLiteral(false)} AND s.\"IsAdult\" = {SqlFormatter.BooleanLiteral(false)}";
                            }

                            //query = (from x in _db.Submissions
                            //         where x.Subverse1.MinCCPForDownvote == 0
                            //                && (!x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value))
                            //                && (x.Subverse1.IsAdult && nsfw || !x.Subverse1.IsAdult)
                            //         select x);
                            break;

                        //for regular subverse queries
                        default:

                            if (!SubverseExists(name))
                            {
                                throw new VoatNotFoundException("Subverse '{0}' not found.", name);
                            }

                            ////Controller Logic:
                            //IQueryable<Submission> submissionsFromASubverseByDate = 
                            //    (from message in _db.Submissions
                            //    join subverse in _db.Subverses on message.Subverse equals subverse.Name
                            //    where !message.IsDeleted && message.Subverse == subverseName
                            //    where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                            //    where !(from bu in _db.SubverseBans where bu.Subverse == subverse.Name select bu.UserName).Contains(message.UserName)
                            //    select message).OrderByDescending(s => s.CreationDate);

                            name = ToCorrectSubverseCasing(name);
                            query.Where = "s.\"Subverse\" = @Name";

                            ////Filter out stickies in subs
                            //query.Append(x => x.Where, "s.ID NOT IN (SELECT sticky.SubmissionID FROM StickiedSubmission sticky WITH (NOLOCK) WHERE sticky.SubmissionID = s.ID AND sticky.Subverse = s.Subverse)");

                            //query = (from x in _db.Submissions
                            //         where (x.Subverse == subverse || subverse == null)
                            //         select x);
                            break;
                    }
                    //Filter out stickies
                    switch (name.ToLower())
                    {
                        //Match Aggregate Subs
                        case AGGREGATE_SUBVERSE.FRONT:
                        case AGGREGATE_SUBVERSE.DEFAULT:
                        case AGGREGATE_SUBVERSE.ANY:
                        case AGGREGATE_SUBVERSE.ALL:
                        case "all":

                            query.Append(x => x.Where, $"s.\"ID\" NOT IN (SELECT sticky.\"SubmissionID\" FROM {SqlFormatter.Table("StickiedSubmission", "sticky", null, "NOLOCK")} WHERE sticky.\"SubmissionID\" = s.\"ID\" AND sticky.\"Subverse\" = 'announcements')");

                            break;
                        //for regular subverse queries
                        default:

                            //Filter out stickies in subs
                            query.Append(x => x.Where, $"s.\"ID\" NOT IN (SELECT sticky.\"SubmissionID\" FROM {SqlFormatter.Table("StickiedSubmission", "sticky", null, "NOLOCK")} WHERE sticky.\"SubmissionID\" = s.\"ID\" AND sticky.\"Subverse\" = s.\"Subverse\")");
                            break;
                    }




                    if (UserIdentity.IsAuthenticated)
                    {
                        if (filterBlockedSubverses)
                        {
                            var set = GetSet(SetType.Blocked.ToString(), userName, SetType.Blocked);
                            if (set != null)
                            {
                                query.Append(x => x.Where, $"sub.\"ID\" NOT IN (SELECT \"SubverseID\" FROM {SqlFormatter.Table("SubverseSetList")} WHERE \"SubverseSetID\" = @BlockedSetID)");
                                query.Parameters.Add("BlockedSetID", set.ID);
                            }
                        }
                    }
                    break;

                case DomainType.Set:
                    joinSet(query, name, ownerName, null, true);

                    if (name.IsEqual("Default") && String.IsNullOrEmpty(ownerName))
                    {
                        ////sort default by relative rank on default if sorted by rank by default
                        //options.Sort = (options.Sort == SortAlgorithm.Rank ? Domain.Models.SortAlgorithm.Relative : options.Sort);

                        if (Settings.IsVoatBranded && options.Sort == SortAlgorithm.Relative)
                        {
                            //This is a modification Voat uses in the default page
                            //Postgre Port
                            //query.Append(x => x.Where, "(s.\"UpCount\" - s.\"DownCount\" >= 20) AND ABS(DATEDIFF(HH, s.\"CreationDate\", GETUTCDATE())) <= 24");
                            query.Append(x => x.Where, "(s.\"UpCount\" - s.\"DownCount\" >= 20) AND s.\"CreationDate\" >= @EndDate");
                            query.Parameters.Add("EndDate", CurrentDate.AddHours(-24));
                       }
                    }

                    break;
            }

            query.Append(x => x.Where, $"s.\"IsDeleted\" = {SqlFormatter.BooleanLiteral(false)}");

            //TODO: Re-implement this logic
            //HACK: Warning, Super hacktastic
            if (!String.IsNullOrEmpty(options.Phrase))
            {
                query.Append(x => x.Where, "(s.\"Title\" LIKE CONCAT('%', @Phrase, '%') OR s.\"Content\" LIKE CONCAT('%', @Phrase, '%') OR s.\"Url\" LIKE CONCAT('%', @Phrase, '%'))");
                ////WARNING: This is a quickie that views spaces as AND conditions in a search.
                //List<string> keywords = null;
                //if (options.Phrase.Contains(" "))
                //{
                //    keywords = new List<string>(options.Phrase.Split(' '));
                //}
                //else
                //{
                //    keywords = new List<string>(new string[] { options.Phrase });
                //}

                //keywords.ForEach(x =>
                //{
                //    query = query.Where(m => m.Title.Contains(x) || m.Content.Contains(x) || m.Url.Contains(x));
                //});
            }

            ApplySubmissionSort(query, options);

            query.SkipCount = options.Index;
            query.TakeCount = options.Count;

            //Filter out all disabled subs
            query.Append(x => x.Where, $"sub.\"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)}");

            query.Parameters.Add("StartDate", startDate);
            query.Parameters.Add("EndDate", endDate);
            query.Parameters.Add("Name", name);
            query.Parameters.Add("UserName", userName);
            query.Parameters.Add("Phrase", options.Phrase);

            //execute query
            var queryString = query.ToString();

            var data = await _db.Connection.QueryAsync<Data.Models.Submission>(queryString, query.Parameters);
            var results = data.Select(Selectors.SecureSubmission).ToList();
            return results;
        }
        private void ApplySubmissionSort(DapperQuery query, SearchOptions options)
        {
            #region Ordering


            if (options.StartDate.HasValue)
            {
                query.Where += " AND s.\"CreationDate\" >= @StartDate";
                //query = query.Where(x => x.CreationDate >= options.StartDate.Value);
            }
            if (options.EndDate.HasValue)
            {
                query.Where += " AND s.\"CreationDate\" <= @EndDate";
                //query = query.Where(x => x.CreationDate <= options.EndDate.Value);
            }

            //Search Options
            switch (options.Sort)
            {
                //case SortAlgorithm.RelativeRank:
                case SortAlgorithm.Relative:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"RelativeRank\" ASC";
                        //query = query.OrderBy(x => x.RelativeRank);
                    }
                    else
                    {
                        query.OrderBy = "s.\"RelativeRank\" DESC";
                        //query = query.OrderByDescending(x => x.RelativeRank);
                    }
                    break;

                case SortAlgorithm.Rank:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"Rank\" ASC";
                        //query = query.OrderBy(x => x.Rank);
                    }
                    else
                    {
                        query.OrderBy = "s.\"Rank\" DESC";
                        //query = query.OrderByDescending(x => x.Rank);
                    }
                    break;

                case SortAlgorithm.Top:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"UpCount\" ASC";
                        //query = query.OrderBy(x => x.UpCount);
                    }
                    else
                    {
                        query.OrderBy = "s.\"UpCount\" DESC";
                        //query = query.OrderByDescending(x => x.UpCount);
                    }
                    break;

                case SortAlgorithm.Viewed:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"Views\" ASC";
                        //query = query.OrderBy(x => x.Views);
                    }
                    else
                    {
                        query.OrderBy = "s.\"Views\" DESC";
                        //query = query.OrderByDescending(x => x.Views);
                    }
                    break;
                //Need to verify performance of these before using
                //case SortAlgorithm.Discussed:
                //    if (options.SortDirection == SortDirection.Reverse)
                //    {
                //        query = query.OrderBy(x => x.Comments.Count);
                //    }
                //    else
                //    {
                //        query = query.OrderByDescending(x => x.Comments.Count);
                //    }
                //    break;

                //case SortAlgorithm.Active:
                //    if (options.SortDirection == SortDirection.Reverse)
                //    {
                //        query = query.OrderBy(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                //    }
                //    else
                //    {
                //        query = query.OrderByDescending(x => x.Comments.OrderBy(c => c.CreationDate).FirstOrDefault().CreationDate);
                //    }
                //    break;

                case SortAlgorithm.Bottom:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"DownCount\" ASC";
                        //query = query.OrderBy(x => x.DownCount);
                    }
                    else
                    {
                        query.OrderBy = "s.\"DownCount\" DESC";
                        //query = query.OrderByDescending(x => x.DownCount);
                    }
                    break;
                //case SortAlgorithm.Active:
                //string activeSort = "s.LastCommentDate";
                ////query.SelectColumns = query.AppendClause(query.SelectColumns, "LastCommentDate = (SELECT TOP 1 ISNULL(c.CreationDate, s.CreationDate) FROM Comment c WITH (NOLOCK) WHERE c.SubmissionID = s.ID ORDER BY c.CreationDate DESC)", ", ");
                //if (options.SortDirection == SortDirection.Reverse)
                //{
                //    query.OrderBy = $"{activeSort} ASC";
                //}
                //else
                //{
                //    query.OrderBy = $"{activeSort} DESC";
                //}
                //break;

                case SortAlgorithm.Intensity:
                    string sort = "(s.\"UpCount\" + s.\"DownCount\")";
                    query.SelectColumns = query.AppendClause(query.SelectColumns, sort, ", ");
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = $"{sort} ASC";
                        //query = query.OrderBy(x => x.UpCount + x.DownCount);
                    }
                    else
                    {
                        query.OrderBy = $"{sort} DESC";
                        //query = query.OrderByDescending(x => x.UpCount + x.DownCount);
                    }
                    break;

                //making this default for easy debugging
                case SortAlgorithm.New:
                default:
                    if (options.SortDirection == SortDirection.Reverse)
                    {
                        query.OrderBy = "s.\"CreationDate\" ASC";
                        //query = query.OrderBy(x => x.CreationDate);
                    }
                    else
                    {
                        query.OrderBy = "s.\"CreationDate\" DESC";
                        //query = query.OrderByDescending(x => x.CreationDate);
                    }
                    break;
            }

            //query = query.Skip(options.Index).Take(options.Count);
            //return query;

            #endregion
        }
        //[Obsolete("Moving to Dapper, see yall later", true)]
        //public async Task<IEnumerable<Models.Submission>> GetSubmissions(string subverse, SearchOptions options)
        //{
        //    if (String.IsNullOrEmpty(subverse))
        //    {
        //        throw new VoatValidationException("A subverse must be provided.");
        //    }

        //    if (options == null)
        //    {
        //        options = new SearchOptions();
        //    }

        //    IQueryable<Models.Submission> query;

        //    UserData userData = null;
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        userData = new UserData(UserIdentity.UserName);
        //    }

        //    switch (subverse.ToLower())
        //    {
        //        //for *special* subverses, this is UNDONE
        //        case AGGREGATE_SUBVERSE.FRONT:
        //            if (User.Identity.IsAuthenticated && userData.HasSubscriptions())
        //            {
        //                query = (from x in _db.Submissions
        //                         join subscribed in _db.SubverseSubscriptions on x.Subverse equals subscribed.Subverse
        //                         where subscribed.UserName == UserIdentity.UserName
        //                         select x);
        //            }
        //            else
        //            {
        //                //if no user, default to default
        //                query = (from x in _db.Submissions
        //                         join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
        //                         select x);
        //            }
        //            break;

        //        case AGGREGATE_SUBVERSE.DEFAULT:

        //            query = (from x in _db.Submissions
        //                     join defaults in _db.DefaultSubverses on x.Subverse equals defaults.Subverse
        //                     select x);
        //            break;

        //        case AGGREGATE_SUBVERSE.ANY:

        //            query = (from x in _db.Submissions
        //                     where
        //                     !x.Subverse1.IsAdminPrivate
        //                     && !x.Subverse1.IsPrivate
        //                     && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value)
        //                     select x);
        //            break;

        //        case AGGREGATE_SUBVERSE.ALL:
        //        case "all":

        //            var nsfw = (User.Identity.IsAuthenticated ? userData.Preferences.EnableAdultContent : false);

        //            //v/all has certain conditions
        //            //1. Only subs that have a MinCCP of zero
        //            //2. Don't show private subs
        //            //3. Don't show NSFW subs if nsfw isn't enabled in profile, if they are logged in
        //            //4. Don't show blocked subs if logged in // not implemented

        //            query = (from x in _db.Submissions
        //                     where x.Subverse1.MinCCPForDownvote == 0
        //                            && (!x.Subverse1.IsAdminPrivate && !x.Subverse1.IsPrivate && !(x.Subverse1.IsAdminDisabled.HasValue && x.Subverse1.IsAdminDisabled.Value))
        //                            && (x.Subverse1.IsAdult && nsfw || !x.Subverse1.IsAdult)
        //                     select x);

        //            break;

        //        //for regular subverse queries
        //        default:

        //            if (!SubverseExists(subverse))
        //            {
        //                throw new VoatNotFoundException("Subverse '{0}' not found.", subverse);
        //            }

        //            subverse = ToCorrectSubverseCasing(subverse);

        //            query = (from x in _db.Submissions
        //                     where (x.Subverse == subverse || subverse == null)
        //                     select x);
        //            break;
        //    }

        //    query = query.Where(x => !x.IsDeleted);

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        //filter blocked subs
        //        query = query.Where(s => !_db.UserBlockedSubverses.Where(b =>
        //            b.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase)
        //            && b.Subverse.Equals(s.Subverse, StringComparison.OrdinalIgnoreCase)).Any());

        //        //filter blocked users (Currently commented out do to a collation issue)
        //        query = query.Where(s => !_db.UserBlockedUsers.Where(b =>
        //            b.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase)
        //            && s.UserName.Equals(b.BlockUser, StringComparison.OrdinalIgnoreCase)
        //            ).Any());

        //        //filter global banned users
        //        query = query.Where(s => !_db.BannedUsers.Where(b => b.UserName.Equals(s.UserName, StringComparison.OrdinalIgnoreCase)).Any());
        //    }

        //    query = ApplySubmissionSearch(options, query);

        //    //execute query
        //    var data = await query.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

        //    var results = data.Select(Selectors.SecureSubmission).ToList();

        //    return results;
        //}

        [Authorize]
        public async Task<CommandResponse<Domain.Models.Submission>> PostSubmission(UserSubmission userSubmission)
        {
            DemandAuthentication();

            //Load Subverse Object
            //var cmdSubverse = new QuerySubverse(userSubmission.Subverse);
            var subverseObject = _db.Subverse.Where(x => x.Name.Equals(userSubmission.Subverse, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            //Evaluate Rules
            var context = new VoatRuleContext();
            context.Subverse = subverseObject;
            context.PropertyBag.UserSubmission = userSubmission;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.Post, RuleScope.PostSubmission);

            //if rules engine denies bail.
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Domain.Models.Submission>(outcome, null);
            }

            //Save submission
            Models.Submission newSubmission = new Models.Submission();
            newSubmission.UpCount = 1; //https://voat.co/v/PreviewAPI/comments/877596
            newSubmission.UserName = UserIdentity.UserName;
            newSubmission.CreationDate = CurrentDate;
            newSubmission.Subverse = subverseObject.Name;

            //TODO: Should be in rule object
            //If IsAnonymized is NULL, this means subverse allows users to submit either anon or non-anon content
            if (subverseObject.IsAnonymized.HasValue)
            {
                //Trap for a anon submittal in a non-anon subverse
                if (!subverseObject.IsAnonymized.Value && userSubmission.IsAnonymized)
                {
                    return MapRuleOutCome<Domain.Models.Submission>(new RuleOutcome(RuleResult.Denied, "Anon Submission Rule", "9.1", "Subverse does not allow anon content"), null);
                }
                newSubmission.IsAnonymized = subverseObject.IsAnonymized.Value;
            }
            else
            {
                newSubmission.IsAnonymized = userSubmission.IsAnonymized;
            }

            //TODO: Determine if subverse is marked as adult or has NSFW in title
            if (subverseObject.IsAdult || (!userSubmission.IsAdult && Regex.IsMatch(userSubmission.Title, CONSTANTS.NSFW_FLAG, RegexOptions.IgnoreCase)))
            {
                userSubmission.IsAdult = true;
            }
            newSubmission.IsAdult = userSubmission.IsAdult;

            //1: Text, 2: Link
            newSubmission.Type = (int)userSubmission.Type;

            if (userSubmission.Type == SubmissionType.Text)
            {
                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                {
                    userSubmission.Content = ContentProcessor.Instance.Process(userSubmission.Content, ProcessingStage.InboundPreSave, newSubmission);
                }

                newSubmission.Title = userSubmission.Title;
                newSubmission.Content = userSubmission.Content;
                newSubmission.FormattedContent = Formatting.FormatMessage(userSubmission.Content, true);
            }
            else
            {
                newSubmission.Title = userSubmission.Title;
                newSubmission.Url = userSubmission.Url;

                if (subverseObject.IsThumbnailEnabled)
                {
                    // try to generate and assign a thumbnail to submission model
                    newSubmission.Thumbnail = await ThumbGenerator.GenerateThumbFromWebpageUrl(userSubmission.Url).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                }
            }

            //Add User Vote to Submission
            newSubmission.SubmissionVoteTrackers.Add(new SubmissionVoteTracker()
            {
                UserName = newSubmission.UserName,
                VoteStatus = (int)Vote.Up,
                VoteValue = GetVoteValue(subverseObject, newSubmission, Vote.Up),
                IPAddress = null,
                CreationDate = Repository.CurrentDate,
            });
            _db.Submission.Add(newSubmission);

            await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            //This sends notifications by parsing content
            if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            {
                ContentProcessor.Instance.Process(String.Concat(newSubmission.Title, " ", newSubmission.Content), ProcessingStage.InboundPostSave, newSubmission);
            }

            return CommandResponse.Successful(newSubmission.Map());
        }

        [Authorize]
        public async Task<CommandResponse<Models.Submission>> EditSubmission(int submissionID, UserSubmission userSubmission)
        {
            DemandAuthentication();

            if (userSubmission == null || (!userSubmission.HasState && String.IsNullOrEmpty(userSubmission.Content)))
            {
                throw new VoatValidationException("The submission must not be null or have invalid state");
            }

            //if (String.IsNullOrEmpty(submission.Url) && String.IsNullOrEmpty(submission.Content)) {
            //    throw new VoatValidationException("Either a Url or Content must be provided.");
            //}

            var submission = _db.Submission.Where(x => x.ID == submissionID).FirstOrDefault();

            if (submission == null)
            {
                throw new VoatNotFoundException(String.Format("Can't find submission with ID {0}", submissionID));
            }

            if (submission.IsDeleted)
            {
                throw new VoatValidationException("Deleted submissions cannot be edited");
            }

            if (submission.UserName != UserIdentity.UserName)
            {
                throw new VoatSecurityException(String.Format("Submission can not be edited by account"));
            }

            //Evaluate Rules
            var context = new VoatRuleContext();
            context.Subverse = DataCache.Subverse.Retrieve(submission.Subverse);
            context.PropertyBag.UserSubmission = userSubmission;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.EditSubmission);

            //if rules engine denies bail.
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Models.Submission>(outcome, null);
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

            await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            return CommandResponse.FromStatus(Selectors.SecureSubmission(submission), Status.Success, "");
        }

        [Authorize]

        //LOGIC COPIED FROM SubmissionController.DeleteSubmission(int)
        public Models.Submission DeleteSubmission(int submissionID, string reason = null)
        {
            DemandAuthentication();

            var submission = _db.Submission.Find(submissionID);

            if (submission != null && !submission.IsDeleted)
            {
                // delete submission if delete request is issued by submission author
                if (submission.UserName == UserIdentity.UserName)
                {
                    submission.IsDeleted = true;

                    if (submission.Type == (int)SubmissionType.Text)
                    {
                        submission.Content = UserDeletedContentMessage();
                        submission.FormattedContent = Formatting.FormatMessage(submission.Content);
                    }
                    else
                    {
                        submission.Url = "http://voat.co";
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmission.FirstOrDefault(s => s.SubmissionID == submissionID);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmission.Remove(existingSticky);
                    }

                    _db.SaveChanges();
                }

                // delete submission if delete request is issued by subverse moderator
                else if (ModeratorPermission.HasPermission(UserIdentity.UserName, submission.Subverse, ModeratorAction.DeletePosts))
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
                        Moderator = UserIdentity.UserName,
                        Reason = reason,
                        CreationDate = Repository.CurrentDate
                    };

                    _db.SubmissionRemovalLog.Add(removalLog);
                    var contentPath = VoatPathHelper.CommentsPagePath(submission.Subverse, submission.ID);

                    // notify submission author that his submission has been deleted by a moderator
                    var message = new Domain.Models.SendMessage()
                    {
                        Sender = $"v/{submission.Subverse}",
                        Recipient = submission.UserName,
                        Subject = $"Submission {contentPath} deleted",
                        Message = "Your submission [" + contentPath + "](" + contentPath + ") has been deleted by: " +
                                    "@" + UserIdentity.UserName + " on " + Repository.CurrentDate + Environment.NewLine + Environment.NewLine +
                                    "Reason given: " + reason + Environment.NewLine +
                                    "#Original Submission" + Environment.NewLine +
                                    "##" + submission.Title + Environment.NewLine +
                                    (submission.Type == 1 ?
                                        submission.Content
                                    :
                                    "[" + submission.Url + "](" + submission.Url + ")"
                                    )

                    };
                    var cmd = new SendMessageCommand(message, isAnonymized: submission.IsAnonymized);
                    cmd.Execute();

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmission.FirstOrDefault(s => s.SubmissionID == submissionID);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmission.Remove(existingSticky);
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

        private static string UserDeletedContentMessage()
        {
            return "Deleted by author at " + Repository.CurrentDate;
        }

        public async Task<CommandResponse> LogVisit(int submissionID, string clientIpAddress)
        {

            if (!String.IsNullOrEmpty(clientIpAddress))
            {
                try
                {

                    // generate salted hash of client IP address
                    string hash = IpHash.CreateHash(clientIpAddress);

                    // register a new session for this subverse
                    //New logic

                    var exists = $"SELECT st.* FROM  {SqlFormatter.Table("SessionTracker", "st", null, "NOLOCK")} WHERE st.\"SessionID\" = @SessionID AND st.\"Subverse\" = (SELECT \"Subverse\" FROM {SqlFormatter.Table("Submission", null, null, "NOLOCK")} WHERE \"ID\" = @SubmissionID)";

                    var body = $"INSERT INTO {SqlFormatter.Table("SessionTracker")} (\"SessionID\", \"Subverse\", \"CreationDate\") ";
                    body += $"SELECT @SessionID, s.\"Subverse\", @Date FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} WHERE \"ID\" = @SubmissionID ";
                    body += $"AND NOT EXISTS ({exists})";

                    await _db.Connection.ExecuteAsync(body, new { SessionID = hash, SubmissionID = submissionID, Date = CurrentDate }, commandType: System.Data.CommandType.Text).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);


                    exists = $"SELECT COUNT(*) FROM {SqlFormatter.Table("ViewStatistic", "vs", null, "NOLOCK")} WHERE vs.\"SubmissionID\" = @SubmissionID AND vs.\"ViewerID\" = @SessionID";
                    var count = await _db.Connection.ExecuteScalarAsync<int>(exists, new { SessionID = hash, SubmissionID = submissionID }).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    if (count == 0)
                    {
                        var sql = $"INSERT INTO {SqlFormatter.Table("ViewStatistic")} (\"SubmissionID\", \"ViewerID\") VALUES (@SubmissionID, @SessionID) ";
                        await _db.Connection.ExecuteAsync(sql, new { SessionID = hash, SubmissionID = submissionID }).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                        sql = $"UPDATE {SqlFormatter.Table("Submission")} SET \"Views\" = (\"Views\" + 1) WHERE \"ID\" = @SubmissionID ";
                        await _db.Connection.ExecuteAsync(sql, new { SessionID = hash, SubmissionID = submissionID }).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    }

                    //sql = $"IF NOT EXISTS (SELECT * FROM {SqlFormatter.Table("ViewStatistic", "vs", null, "NOLOCK")} WHERE vs.\"SubmissionID\" = @SubmissionID AND vs.\"ViewerID\" = @SessionID) ";
                    //sql += $"BEGIN ";
                    //sql += $"INSERT {SqlFormatter.Table("ViewStatistic")} (\"SubmissionID\", \"ViewerID\") VALUES (@SubmissionID, @SessionID) ";
                    //sql += $"UPDATE {SqlFormatter.Table("Submission")} SET \"Views\" = (\"Views\" + 1) WHERE \"ID\" = @SubmissionID ";
                    //sql += $"END";

                    //await _db.Connection.ExecuteAsync(sql, new { SessionID = hash, SubmissionID = submissionID }).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);


                }
                catch (Exception ex)
                {
                    EventLogger.Instance.Log(ex);
                    throw ex;
                }

            }
            return CommandResponse.FromStatus(Status.Success, "");
        }

        #endregion Submissions

        #region Comments

        public async Task<IEnumerable<Domain.Models.SubmissionComment>> GetUserComments(string userName, SearchOptions options)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new VoatValidationException("A user name must be provided.");
            }
            if (!UserHelper.UserExists(userName))
            {
                throw new VoatValidationException("User '{0}' does not exist.", userName);
            }

            var query = (from comment in _db.Comment
                         join submission in _db.Submission on comment.SubmissionID equals submission.ID
                         where
                            !comment.IsAnonymized
                            && !comment.IsDeleted
                            && (comment.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                         select new Domain.Models.SubmissionComment()
                         {
                             Submission = new SubmissionSummary()
                             {
                                 Title = submission.Title,
                                 IsDeleted = submission.IsDeleted,
                                 IsAnonymized = submission.IsAnonymized,
                                 UserName = (submission.IsAnonymized || submission.IsDeleted ? "" : submission.UserName)
                             },
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
            var results = await query.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            return results;
        }

        public IEnumerable<Domain.Models.SubmissionComment> GetComments(string subverse, SearchOptions options)
        {

            IQueryable<SubmissionComment> query = null;

            //Postgre Port: Postgre has issues with || conditions so as a hack seperating for now
            //var query = (from comment in _db.Comments
            //             join submission in _db.Submissions on comment.SubmissionID equals submission.ID
            //             where
            //             !comment.IsDeleted
            //             && (submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || String.IsNullOrEmpty(subverse))
            //             select new Domain.Models.SubmissionComment()
            //             {
            //                 Submission = new SubmissionSummary()
            //                 {
            //                     Title = submission.Title,
            //                     IsDeleted = submission.IsDeleted,
            //                     IsAnonymized = submission.IsAnonymized,
            //                     UserName = (submission.IsAnonymized || submission.IsDeleted ? "" : submission.UserName)
            //                 },
            //                 ID = comment.ID,
            //                 ParentID = comment.ParentID,
            //                 Content = comment.Content,
            //                 FormattedContent = comment.FormattedContent,
            //                 UserName = comment.UserName,
            //                 UpCount = (int)comment.UpCount,
            //                 DownCount = (int)comment.DownCount,
            //                 CreationDate = comment.CreationDate,
            //                 IsAnonymized = comment.IsAnonymized,
            //                 IsDeleted = comment.IsDeleted,
            //                 IsDistinguished = comment.IsDistinguished,
            //                 LastEditDate = comment.LastEditDate,
            //                 SubmissionID = comment.SubmissionID,
            //                 Subverse = submission.Subverse
            //             });

            if (String.IsNullOrEmpty(subverse))
            {
                query = (from comment in _db.Comment
                         join submission in _db.Submission on comment.SubmissionID equals submission.ID
                         where
                         !comment.IsDeleted
                         //&& (submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || String.IsNullOrEmpty(subverse))
                         select new Domain.Models.SubmissionComment()
                         {
                             Submission = new SubmissionSummary()
                             {
                                 Title = submission.Title,
                                 IsDeleted = submission.IsDeleted,
                                 IsAnonymized = submission.IsAnonymized,
                                 UserName = (submission.IsAnonymized || submission.IsDeleted ? "" : submission.UserName)
                             },
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

            }
            else {
                query = (from comment in _db.Comment
                         join submission in _db.Submission on comment.SubmissionID equals submission.ID
                         where
                         !comment.IsDeleted
                         && (submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase))
                         select new Domain.Models.SubmissionComment()
                         {
                             Submission = new SubmissionSummary()
                             {
                                 Title = submission.Title,
                                 IsDeleted = submission.IsDeleted,
                                 IsAnonymized = submission.IsAnonymized,
                                 UserName = (submission.IsAnonymized || submission.IsDeleted ? "" : submission.UserName)
                             },
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

            }

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

            //Postgre Port
            //var commentTree = _db.usp_CommentTree(submissionID, depth, parentID);

            IEnumerable<usp_CommentTree_Result> commentTree = null;
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:
                    commentTree = _db.Connection.Query<usp_CommentTree_Result>("usp_CommentTree", new { SubmissionID = submissionID, Depth = depth, ParentID = parentID }, commandType: System.Data.CommandType.StoredProcedure);
                    break;
                case DataStoreType.PostgreSql:
                    var d = new DapperQuery();
                    d.Select = "* FROM \"dbo\".\"usp_CommentTree\"(@SubmissionID, @Depth, @ParentID)";
                    commentTree = _db.Connection.Query<usp_CommentTree_Result>(d.ToString(), new { SubmissionID = submissionID, Depth = depth, ParentID = parentID });
                    break;
            }

            var results = commentTree.ToList();
            return results;
        }
        //For backwards compat
        public async Task<Domain.Models.Comment> GetComment(int commentID)
        {
            var result = await GetComments(commentID);
            return result.FirstOrDefault();
        }
        public async Task<IEnumerable<Domain.Models.Comment>> GetComments(params int[] commentID)
        {

            var q = new DapperQuery();
            q.Select = $"c.*, s.\"Subverse\" FROM {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON s.\"ID\" = c.\"SubmissionID\"";
            q.Where = $"c.\"ID\" {SqlFormatter.In("@IDs")}";

            q.Parameters = (new { IDs = commentID }).ToDynamicParameters();

            //var query = (from comment in _db.Comments
            //             join submission in _db.Submissions on comment.SubmissionID equals submission.ID
            //             where
            //             comment.ID == commentID
            //             select new Domain.Models.Comment()
            //             {
            //                 ID = comment.ID,
            //                 ParentID = comment.ParentID,
            //                 Content = comment.Content,
            //                 FormattedContent = comment.FormattedContent,
            //                 UserName = comment.UserName,
            //                 UpCount = (int)comment.UpCount,
            //                 DownCount = (int)comment.DownCount,
            //                 CreationDate = comment.CreationDate,
            //                 IsAnonymized = comment.IsAnonymized,
            //                 IsDeleted = comment.IsDeleted,
            //                 IsDistinguished = comment.IsDistinguished,
            //                 LastEditDate = comment.LastEditDate,
            //                 SubmissionID = comment.SubmissionID,
            //                 Subverse = submission.Subverse
            //             });

            //var record = query.FirstOrDefault();

            var data = await _db.Connection.QueryAsync<Domain.Models.Comment>(q.ToString(), q.Parameters);

            DomainMaps.HydrateUserData(data);

            return data;
        }

        private async Task ResetVotes(ContentType contentType, int id, Vote voteStatus, Vote voteValue)
        {
            var u = new DapperUpdate();
            switch (contentType)
            {
                case ContentType.Comment:
                    u.Update = $"UPDATE v SET v.\"VoteValue\" = @VoteValue FROM {SqlFormatter.Table("CommentVoteTracker", "v")} INNER JOIN {SqlFormatter.Table("Comment", "c", "NOLOCK")} ON c.\"ID\" = v.\"CommentID\" INNER JOIN {SqlFormatter.Table("Submission", "s", "NOLOCK")}  ON c.\"SubmissionID\" = s.\"ID\"";
                    u.Where = "v.CommentID = @ID AND v.VoteStatus = @VoteStatus AND s.ArchiveDate IS NULL";
                    break;
                default:
                    throw new NotImplementedException($"Method not implemented for ContentType: {contentType.ToString()}");
                    break;
            }
            int count = await _db.Connection.ExecuteAsync(u.ToString(), new { ID = id, VoteStatus = (int)voteStatus, VoteValue = (int)voteValue });
        }

        public async Task<CommandResponse<Data.Models.Comment>> DeleteComment(int commentID, string reason = null)
        {
            DemandAuthentication();

            var comment = _db.Comment.Find(commentID);

            if (comment != null && !comment.IsDeleted)
            {
                var submission = _db.Submission.Find(comment.SubmissionID);
                if (submission != null)
                {
                    var subverseName = submission.Subverse;

                    // delete comment if the comment author is currently logged in user
                    if (comment.UserName == UserIdentity.UserName)
                    {
                        //User Deletion
                        comment.IsDeleted = true;
                        comment.Content = UserDeletedContentMessage();
                        comment.FormattedContent = Formatting.FormatMessage(comment.Content);
                        await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        //User Deletions remove UpVoted CCP - This is one way ccp farmers accomplish their acts
                        if (comment.UpCount > comment.DownCount)
                        {
                            await ResetVotes(ContentType.Comment, comment.ID, Vote.Up, Vote.None).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                        }
                    }

                    // delete comment if delete request is issued by subverse moderator
                    else if (ModeratorPermission.HasPermission(UserIdentity.UserName, submission.Subverse, ModeratorAction.DeleteComments))
                    {
                        if (String.IsNullOrEmpty(reason))
                        {
                            var ex = new VoatValidationException("A reason for deletion is required");
                            ex.Data["CommentID"] = commentID;
                            throw ex;
                        }
                        var contentPath = VoatPathHelper.CommentsPagePath(submission.Subverse, comment.SubmissionID.Value, comment.ID);

                        // notify comment author that his comment has been deleted by a moderator
                        var message = new Domain.Models.SendMessage()
                        {
                            Sender = $"v/{subverseName}",
                            Recipient = comment.UserName,
                            Subject = $"Comment {contentPath} deleted",
                            Message = "Your comment [" + contentPath + "](" + contentPath + ") has been deleted by: " +
                                        "@" + UserIdentity.UserName + " on: " + Repository.CurrentDate + Environment.NewLine + Environment.NewLine +
                                        "Reason given: " + reason + Environment.NewLine +
                                        "#Original Comment" + Environment.NewLine +
                                        comment.Content
                        };
                        var cmd = new SendMessageCommand(message, isAnonymized: comment.IsAnonymized);
                        await cmd.Execute().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        comment.IsDeleted = true;

                        // move the comment to removal log
                        var removalLog = new Data.Models.CommentRemovalLog
                        {
                            CommentID = comment.ID,
                            Moderator = UserIdentity.UserName,
                            Reason = reason,
                            CreationDate = Repository.CurrentDate
                        };

                        _db.CommentRemovalLog.Add(removalLog);

                        await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    }
                    else
                    {
                        var ex = new VoatSecurityException("User doesn't have permissions to perform requested action");
                        ex.Data["CommentID"] = commentID;
                        throw ex;
                    }
                }
            }
            return CommandResponse.Successful(Selectors.SecureComment(comment));
        }

        [Authorize]
        public async Task<CommandResponse<Data.Models.Comment>> EditComment(int commentID, string content)
        {
            DemandAuthentication();

            var comment = _db.Comment.Find(commentID);

            if (comment != null)
            {
                if (comment.UserName != UserIdentity.UserName)
                {
                    return CommandResponse.FromStatus((Data.Models.Comment)null, Status.Denied, "User doesn't have permissions to perform requested action");
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

                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                }
                else
                {
                    return MapRuleOutCome<Data.Models.Comment>(outcome, null);
                }
            }
            else
            {
                throw new VoatNotFoundException("Can not find comment with ID {0}", commentID);
            }

            return CommandResponse.Successful(Selectors.SecureComment(comment));
        }

        public async Task<CommandResponse<Domain.Models.Comment>> PostCommentReply(int parentCommentID, string comment)
        {
            var c = _db.Comment.Find(parentCommentID);
            if (c == null)
            {
                throw new VoatNotFoundException("Can not find parent comment with id {0}", parentCommentID.ToString());
            }
            var submissionid = c.SubmissionID;
            return await PostComment(submissionid.Value, parentCommentID, comment).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
        }

        public async Task<CommandResponse<Domain.Models.Comment>> PostComment(int submissionID, int? parentCommentID, string commentContent)
        {
            DemandAuthentication();

            var submission = GetSubmissionUnprotected(submissionID);
            if (submission == null)
            {
                throw new VoatNotFoundException("submissionID", submissionID, "Can not find submission");
            }

            //evaluate rule
            VoatRuleContext context = new VoatRuleContext();

            //set any state we have so context doesn't have to retrieve
            context.SubmissionID = submissionID;
            context.PropertyBag.CommentContent = commentContent;

            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.Post, RuleScope.PostComment);

            if (outcome.IsAllowed)
            {
                //Save comment
                var c = new Models.Comment();
                c.CreationDate = Repository.CurrentDate;
                c.UserName = UserIdentity.UserName;
                c.ParentID = (parentCommentID > 0 ? parentCommentID : (int?)null);
                c.SubmissionID = submissionID;
                c.UpCount = 0;

                //TODO: Ensure this is acceptable
                //c.IsAnonymized = (submission.IsAnonymized || subverse.IsAnonymized);
                c.IsAnonymized = submission.IsAnonymized;

                c.Content = ContentProcessor.Instance.Process(commentContent, ProcessingStage.InboundPreSave, c);

                //save fully formatted content
                var formattedComment = Formatting.FormatMessage(c.Content);
                c.FormattedContent = formattedComment;

                _db.Comment.Add(c);
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                {
                    ContentProcessor.Instance.Process(c.Content, ProcessingStage.InboundPostSave, c);
                }

                await NotificationManager.SendCommentNotification(submission, c).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                return MapRuleOutCome(outcome, DomainMaps.Map(Selectors.SecureComment(c), submission.Subverse));
            }

            return MapRuleOutCome(outcome, (Domain.Models.Comment)null);
        }

        #endregion Comments

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
            var result = (from x in this._db.ApiClient
                          where x.PublicKey == apiPublicKey
                          select x).FirstOrDefault();
            return result;
        }

        [Authorize]
        public IEnumerable<ApiClient> GetApiKeys(string userName)
        {
            var result = from x in this._db.ApiClient
                         where x.UserName == userName
                         orderby x.IsActive descending, x.CreationDate descending
                         select x;
            return result.ToList();
        }

        [Authorize]
        public ApiThrottlePolicy GetApiThrottlePolicy(int throttlePolicyID)
        {
            var result = from policy in _db.ApiThrottlePolicy
                         where policy.ID == throttlePolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public ApiPermissionPolicy GetApiPermissionPolicy(int permissionPolicyID)
        {
            var result = from policy in _db.ApiPermissionPolicy
                         where policy.ID == permissionPolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public List<KeyValuePair<string, string>> GetApiClientKeyThrottlePolicies()
        {
            List<KeyValuePair<string, string>> policies = new List<KeyValuePair<string, string>>();

            var result = from client in this._db.ApiClient
                         join policy in _db.ApiThrottlePolicy on client.ApiThrottlePolicyID equals policy.ID
                         where client.IsActive == true
                         select new { client.PublicKey, policy.Policy };

            foreach (var policy in result)
            {
                policies.Add(new KeyValuePair<string, string>(policy.PublicKey, policy.Policy));
            }

            return policies;
        }

        public async Task<ApiClient> EditApiKey(string apiKey, string name, string description, string url, string redirectUrl)
        {
            DemandAuthentication();

            //Only allow users to edit ApiKeys if they IsActive == 1 and Current User owns it.
            var apiClient = (from x in _db.ApiClient
                             where x.PublicKey == apiKey && x.UserName == UserIdentity.UserName && x.IsActive == true
                             select x).FirstOrDefault();

            if (apiClient != null)
            {
                apiClient.AppAboutUrl = url;
                apiClient.RedirectUrl = redirectUrl;
                apiClient.AppDescription = description;
                apiClient.AppName = name;
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            }

            return apiClient;

        }

        [Authorize]
        public void CreateApiKey(string name, string description, string url, string redirectUrl)
        {
            DemandAuthentication();

            ApiClient c = new ApiClient();
            c.IsActive = true;
            c.AppAboutUrl = url;
            c.RedirectUrl = redirectUrl;
            c.AppDescription = description;
            c.AppName = name;
            c.UserName = UserIdentity.UserName;
            c.CreationDate = CurrentDate;

            var generatePublicKey = new Func<string>(() =>
            {
                return String.Format("VO{0}AT", Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            });

            //just make sure key isn't already in db
            var publicKey = generatePublicKey();
            while (_db.ApiClient.Any(x => x.PublicKey == publicKey))
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

            _db.ApiClient.Add(c);
            _db.SaveChanges();
        }

        [Authorize]
        public ApiClient DeleteApiKey(int id)
        {
            DemandAuthentication();

            //Only allow users to delete ApiKeys if they IsActive == 1
            var apiKey = (from x in _db.ApiClient
                          where x.ID == id && x.UserName == UserIdentity.UserName && x.IsActive == true
                          select x).FirstOrDefault();

            if (apiKey != null)
            {
                apiKey.IsActive = false;
                _db.SaveChanges();
            }
            return apiKey;
        }

        public IEnumerable<ApiCorsPolicy> GetApiCorsPolicies()
        {
            var policy = (from x in _db.ApiCorsPolicy
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

            var policy = (from x in _db.ApiCorsPolicy

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
            _db.ApiLog.Add(logentry);
            _db.SaveChanges();
        }

        public void UpdateApiClientLastAccessDate(int apiClientID)
        {
            var client = _db.ApiClient.Where(x => x.ID == apiClientID).FirstOrDefault();
            client.LastAccessDate = CurrentDate;
            _db.SaveChanges();
        }

        #endregion Api

        #region UserMessages

        public async Task<IEnumerable<int>> GetUserSavedItems(ContentType type, string userName)
        {
            List<int> savedIDs = null;
            switch (type)
            {
                case ContentType.Comment:
                    savedIDs = await _db.CommentSaveTracker.Where(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)).Select(x => x.CommentID).ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    break;
                case ContentType.Submission:
                    savedIDs = await _db.SubmissionSaveTracker.Where(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)).Select(x => x.SubmissionID).ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    break;
            }
            return savedIDs;
        }

        /// <summary>
        /// Save Comments and Submissions toggle.
        /// </summary>
        /// <param name="type">The type of content in which to save</param>
        /// <param name="ID">The ID of the item in which to save</param>
        /// <param name="forceAction">Forces the Save function to operate as a Save only or Unsave only rather than a toggle. If true, will only save if it hasn't been previously saved, if false, will only remove previous saved entry, if null (default) will function as a toggle.</param>
        /// <returns>The end result if the item is saved or not. True if saved, false if not saved.</returns>
        public async Task<CommandResponse<bool?>> Save(ContentType type, int ID, bool? forceAction = null)
        {
            //TODO: These save trackers should be stored in a single table in SQL. Two tables for such similar information isn't ideal... mmkay. Makes querying nasty.
            //TODO: There is a potential issue with this code. There is no validation that the ID belongs to a comment or a submission. This is nearly impossible to determine anyways but it's still an issue.
            string currentUserName = UserIdentity.UserName;
            bool isSaved = false;

            switch (type)
            {
                case ContentType.Comment:

                    var c = _db.CommentSaveTracker.FirstOrDefault(x => x.CommentID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));

                    if (c == null && (forceAction == null || forceAction.HasValue && forceAction.Value))
                    {
                        c = new CommentSaveTracker() { CommentID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.CommentSaveTracker.Add(c);
                        isSaved = true;
                    }
                    else if (c != null && (forceAction == null || forceAction.HasValue && !forceAction.Value))
                    {
                        _db.CommentSaveTracker.Remove(c);
                        isSaved = false;
                    }
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    break;

                case ContentType.Submission:

                    var s = _db.SubmissionSaveTracker.FirstOrDefault(x => x.SubmissionID == ID && x.UserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
                    if (s == null && (forceAction == null || forceAction.HasValue && forceAction.Value))
                    {
                        s = new SubmissionSaveTracker() { SubmissionID = ID, UserName = currentUserName, CreationDate = CurrentDate };
                        _db.SubmissionSaveTracker.Add(s);
                        isSaved = true;
                    }
                    else if (s != null && (forceAction == null || forceAction.HasValue && !forceAction.Value))
                    {
                        _db.SubmissionSaveTracker.Remove(s);
                        isSaved = false;
                    }
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    break;
            }

            return CommandResponse.FromStatus<bool?>(forceAction.HasValue ? forceAction.Value : isSaved, Status.Success, "");
        }

        public static void SetDefaultUserPreferences(Data.Models.UserPreference p)
        {
            p.Language = "en";
            p.NightMode = false;
            //p.OpenInNewWindow = false;
            p.UseSubscriptionsMenu = true;
            p.DisableCSS = false;
            p.DisplaySubscriptions = false;
            p.DisplayVotes = false;
            p.EnableAdultContent = false;
            p.Bio = null;
            p.Avatar = null;
            p.CollapseCommentLimit = 4;
            p.DisplayCommentCount = 5;
            p.HighlightMinutes = 30;
            p.VanityTitle = null;
        }

        [Authorize]
        public void SaveUserPrefernces(Domain.Models.UserPreferenceUpdate preferences)
        {
            DemandAuthentication();

            var p = (from x in _db.UserPreference
                     where x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase)
                     select x).FirstOrDefault();

            if (p == null)
            {
                p = new Data.Models.UserPreference();
                p.UserName = UserIdentity.UserName;
                SetDefaultUserPreferences(p);
                _db.UserPreference.Add(p);
            }

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
            if (preferences.BlockAnonymized.HasValue)
            {
                p.BlockAnonymized = preferences.BlockAnonymized.Value;
            }
            if (preferences.CommentSort != null)
            {
                p.CommentSort = (int)preferences.CommentSort.Value;
            }
            //if (Extensions.IsValidEnumValue(preferences.CommentSort))
            //{
            //    p.CommentSort = (int)preferences.CommentSort.Value;
            //}
            _db.SaveChanges();
        }

        [Authorize]
        public async Task<CommandResponse<Domain.Models.Message>> SendMessageReply(int id, string messageContent)
        {
            DemandAuthentication();

            var userName = UserIdentity.UserName;

            var m = (from x in _db.Message
                     where x.ID == id
                     select x).FirstOrDefault();

            if (m == null)
            {
                return new CommandResponse<Domain.Models.Message>(null, Status.NotProcessed, "Couldn't find message in which to reply");
            }
            else
            {
                var message = new Domain.Models.Message();
                CommandResponse<Domain.Models.Message> commandResponse = null;

                //determine if message replying to is a comment and if so execute a comment reply
                switch ((MessageType)m.Type)
                {
                    case MessageType.CommentMention:
                    case MessageType.CommentReply:
                    case MessageType.SubmissionReply:
                    case MessageType.SubmissionMention:

                        Domain.Models.Comment comment;
                        //assume every comment type has a submission ID contained in it
                        var cmd = new CreateCommentCommand(m.SubmissionID.Value, m.CommentID, messageContent);
                        var response = await cmd.Execute();

                        if (response.Success)
                        {
                            comment = response.Response;
                            commandResponse = CommandResponse.Successful(new Domain.Models.Message()
                            {
                                ID = -1,
                                Comment = comment,
                                SubmissionID = comment.SubmissionID,
                                CommentID = comment.ID,
                                Content = comment.Content,
                                FormattedContent = comment.FormattedContent,
                            });
                        }
                        else
                        {
                            commandResponse = CommandResponse.FromStatus<Domain.Models.Message>(null, response.Status, response.Message);
                        }

                        break;
                    case MessageType.Sent:
                        //no replying to sent messages
                        commandResponse = CommandResponse.FromStatus<Domain.Models.Message>(null, Status.Denied, "Sent messages do not allow replies");
                        break;
                    default:

                        if (m.RecipientType == (int)IdentityType.Subverse)
                        {
                            if (!ModeratorPermission.HasPermission(UserIdentity.UserName, m.Recipient, ModeratorAction.SendMail))
                            {
                                commandResponse = new CommandResponse<Domain.Models.Message>(null, Status.NotProcessed, "Message integrity violated");
                            }

                            message.Recipient = m.Sender;
                            message.RecipientType = (IdentityType)m.SenderType;

                            message.Sender = m.Recipient;
                            message.SenderType = (IdentityType)m.RecipientType;
                        }
                        else
                        {
                            message.Recipient = m.Sender;
                            message.RecipientType = (IdentityType)m.SenderType;

                            message.Sender = m.Recipient;
                            message.SenderType = (IdentityType)m.RecipientType;
                        }

                        message.ParentID = m.ID;
                        message.CorrelationID = m.CorrelationID;
                        message.Title = m.Title;
                        message.Content = messageContent;
                        message.FormattedContent = Formatting.FormatMessage(messageContent);
                        message.IsAnonymized = m.IsAnonymized;
                        commandResponse = await SendMessage(message).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        break;
                }
                //return response
                return commandResponse;
            }
        }

        [Authorize]
        public async Task<IEnumerable<CommandResponse<Domain.Models.Message>>> SendMessages(params Domain.Models.Message[] messages)
        {
            return await SendMessages(messages.AsEnumerable()).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
        }

        [Authorize]
        public async Task<IEnumerable<CommandResponse<Domain.Models.Message>>> SendMessages(IEnumerable<Domain.Models.Message> messages)
        {
            var tasks = messages.Select(x => Task.Run(async () => { return await SendMessage(x).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT); }));

            var result = await Task.WhenAll(tasks).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            return result;
        }


        /// <summary>
        /// Main SendMessage routine.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<CommandResponse<Domain.Models.Message>> SendMessage(Domain.Models.Message message)
        {
            DemandAuthentication();

            using (var db = new VoatDataContext())
            {
                try
                {
                    List<Domain.Models.Message> messages = new List<Domain.Models.Message>();

                    //increased subject line
                    int max = 500;
                    message.CreatedBy = UserIdentity.UserName;
                    message.Title = message.Title.SubstringMax(max);
                    message.CreationDate = CurrentDate;
                    message.FormattedContent = Formatting.FormatMessage(message.Content);

                    //Prevent abuse in subverse ban/unban/etc flooding by checking against the sender in subverse messages
                    var sender = message.Sender;
                    if (message.SenderDefinition.Type == IdentityType.Subverse)
                    {
                        sender = message.CreatedBy;
                    }

                    if (!MesssagingUtility.IsSenderBlocked(sender, message.Recipient))
                    {
                        messages.Add(message);
                    }

                    if (message.Type == MessageType.Private)
                    {
                        //add sent copy
                        var sentCopy = message.Clone();
                        sentCopy.Type = MessageType.Sent;
                        sentCopy.ReadDate = CurrentDate;
                        messages.Add(sentCopy);
                    }

                    var mappedDataMessages = messages.Map();
                    db.Message.AddRange(mappedDataMessages);
                    var addedMessages = db.Message;

                    await db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    //send notices async
                    Task.Run(() => EventNotification.Instance.SendMessageNotice(
                        UserDefinition.Format(message.Recipient, message.RecipientType),
                        UserDefinition.Format(message.Sender, message.SenderType),
                        Domain.Models.MessageTypeFlag.Private,
                        null,
                        null,
                        message.Content));

                    return CommandResponse.Successful(addedMessages.First().Map());
                }
                catch (Exception ex)
                {
                    //TODO Log this
                    return CommandResponse.Error<CommandResponse<Domain.Models.Message>>(ex);
                }
            }
        }

        [Authorize]
        public async Task<CommandResponse<Domain.Models.Message>> SendMessage(SendMessage message, bool forceSend = false, bool ensureUserExists = true, bool isAnonymized = false)
        {
            DemandAuthentication();

            Domain.Models.Message responseMessage = null;

            var sender = UserDefinition.Parse(message.Sender);

            //If sender isn't a subverse (automated messages) run sender checks
            if (sender.Type == IdentityType.Subverse)
            {
                var subverse = sender.Name;
                if (!ModeratorPermission.HasPermission(UserIdentity.UserName, subverse, ModeratorAction.SendMail))
                {
                    return CommandResponse.FromStatus(responseMessage, Status.Denied, "User not allowed to send mail from subverse");
                }
            }
            else
            {
                //Sender can be passed in from the UI , ensure it is replaced here
                message.Sender = UserIdentity.UserName;

                if (Voat.Utilities.BanningUtility.ContentContainsBannedDomain(null, message.Message))
                {
                    return CommandResponse.FromStatus(responseMessage, Status.Ignored, "Message contains banned domain");
                }
                if (Voat.Utilities.UserHelper.IsUserGloballyBanned(message.Sender))
                {
                    return CommandResponse.FromStatus(responseMessage, Status.Ignored, "User is banned");
                }
                var userData = new UserData(message.Sender);
                //add exception for system messages from sender
                var minCCPToSendMessages = Settings.MinimumCommentPointsForSendingMessages;

                if (!forceSend && !CONSTANTS.SYSTEM_USER_NAME.Equals(message.Sender, StringComparison.OrdinalIgnoreCase) && userData.Information.CommentPoints.Sum < minCCPToSendMessages)
                {
                    return CommandResponse.FromStatus(responseMessage, Status.Ignored, $"Comment points too low to send messages. Need at least {minCCPToSendMessages} CCP.");
                }
            }

            List<Domain.Models.Message> messages = new List<Domain.Models.Message>();

            var userDefinitions = UserDefinition.ParseMany(message.Recipient);
            if (userDefinitions.Count() <= 0)
            {
                return CommandResponse.FromStatus(responseMessage, Status.NotProcessed, "No recipient specified");
            }

            foreach (var def in userDefinitions)
            {
                if (def.Type == IdentityType.Subverse)
                {
                    messages.Add(new Domain.Models.Message
                    {
                        CorrelationID = Domain.Models.Message.NewCorrelationID(),
                        Sender = sender.Name,
                        SenderType = sender.Type,
                        Recipient = def.Name,
                        RecipientType = IdentityType.Subverse,
                        Title = message.Subject,
                        Content = message.Message,
                        ReadDate = null,
                        CreationDate = Repository.CurrentDate,
                        IsAnonymized = isAnonymized,
                    });
                }
                else
                {
                    //ensure proper cased, will return null if doesn't exist
                    var recipient = UserHelper.OriginalUsername(def.Name);
                    if (String.IsNullOrEmpty(recipient))
                    {
                        if (ensureUserExists)
                        {
                            return CommandResponse.FromStatus<Domain.Models.Message>(null, Status.Error, $"User {recipient} does not exist.");
                        }
                    }
                    else
                    {
                        messages.Add(new Domain.Models.Message
                        {
                            CorrelationID = Domain.Models.Message.NewCorrelationID(),
                            Sender = sender.Name,
                            SenderType = sender.Type,
                            Recipient = recipient,
                            RecipientType = IdentityType.User,
                            Title = message.Subject,
                            Content = message.Message,
                            ReadDate = null,
                            CreationDate = Repository.CurrentDate,
                            IsAnonymized = isAnonymized,
                        });
                    }
                }
            }

            var savedMessages = await SendMessages(messages).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            var firstSent = savedMessages.FirstOrDefault();
            if (firstSent == null)
            {
                firstSent = CommandResponse.FromStatus((Domain.Models.Message)null, Status.Invalid, "No messages sent. Please confirm that message details are valid.");
            }
            return firstSent;
        }

        [Obsolete("Packing up and moving to Dapper", true)]
        private IQueryable<Data.Models.Message> GetMessageQueryBase(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
        {
            return GetMessageQueryBase(_db, ownerName, ownerType, type, state);
        }
        private DapperQuery GetMessageQueryDapperBase(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
        {
            var q = new DapperQuery();

            q.Select = $"SELECT {"{0}"} FROM {SqlFormatter.Table("Message", "m", null, "NOLOCK")} LEFT JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON s.\"ID\" = m.\"SubmissionID\" LEFT JOIN {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} ON c.\"ID\" = m.\"CommentID\"";
            q.SelectColumns = "*";
            string senderClause = "";

            //messages include sent items, add special condition to include them
            if ((type & MessageTypeFlag.Sent) > 0)
            {
                senderClause = $" OR (m.\"Sender\" = @OwnerName AND m.\"SenderType\" = @OwnerType AND m.\"Type\" = {((int)MessageType.Sent).ToString()})";
            }
            q.Where = $"((m.\"Recipient\" = @OwnerName AND m.\"RecipientType\" = @OwnerType AND m.\"Type\" != {((int)MessageType.Sent).ToString()}){senderClause})";
            q.OrderBy = "m.\"CreationDate\" DESC";
            q.Parameters.Add("OwnerName", ownerName);
            q.Parameters.Add("OwnerType", (int)ownerType);

            //var q = (from m in context.Messages
            //             //join s in _db.Submissions on m.SubmissionID equals s.ID into ns
            //             //from s in ns.DefaultIfEmpty()
            //             //join c in _db.Comments on m.CommentID equals c.ID into cs
            //             //from c in cs.DefaultIfEmpty()
            //         where (
            //            (m.Recipient.Equals(ownerName, StringComparison.OrdinalIgnoreCase) && m.RecipientType == (int)ownerType && m.Type != (int)MessageType.Sent)
            //            ||
            //            //Limit sent messages
            //            (m.Sender.Equals(ownerName, StringComparison.OrdinalIgnoreCase) && m.SenderType == (int)ownerType && ((type & MessageTypeFlag.Sent) > 0) && m.Type == (int)MessageType.Sent)
            //         )
            //         select m);

            switch (state)
            {
                case MessageState.Read:
                    q.Append(x => x.Where, "m.\"ReadDate\" IS NOT NULL");
                    break;
                case MessageState.Unread:
                    q.Append(x => x.Where, "m.\"ReadDate\" IS NULL");
                    break;
            }

            //filter Message Type
            if (type != MessageTypeFlag.All)
            {
                List<int> messageTypes = new List<int>();

                var flags = Enum.GetValues(typeof(MessageTypeFlag));
                foreach (var flag in flags)
                {
                    //This needs to be cleaned up, we have two enums that are serving a similar purpose
                    var mTFlag = (MessageTypeFlag)flag;
                    if (mTFlag != MessageTypeFlag.All && ((type & mTFlag) > 0))
                    {
                        switch (mTFlag)
                        {
                            case MessageTypeFlag.Sent:
                                messageTypes.Add((int)MessageType.Sent);
                                break;

                            case MessageTypeFlag.Private:
                                messageTypes.Add((int)MessageType.Private);
                                break;

                            case MessageTypeFlag.CommentReply:
                                messageTypes.Add((int)MessageType.CommentReply);
                                break;

                            case MessageTypeFlag.CommentMention:
                                messageTypes.Add((int)MessageType.CommentMention);
                                break;

                            case MessageTypeFlag.SubmissionReply:
                                messageTypes.Add((int)MessageType.SubmissionReply);
                                break;

                            case MessageTypeFlag.SubmissionMention:
                                messageTypes.Add((int)MessageType.SubmissionMention);
                                break;
                        }
                    }
                }
                q.Append(x => x.Where, $"m.\"Type\" {SqlFormatter.In("@Types")}");
                q.Parameters.Add("Types", messageTypes.ToArray());
                //q = q.Where(x => messageTypes.Contains(x.Type));
            }
            return q;
        }

        [Obsolete("Packing up and moving to Dapper", true)]
        private IQueryable<Data.Models.Message> GetMessageQueryBase(VoatDataContext context, string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
        {
            var q = (from m in context.Message
                         //join s in _db.Submissions on m.SubmissionID equals s.ID into ns
                         //from s in ns.DefaultIfEmpty()
                         //join c in _db.Comments on m.CommentID equals c.ID into cs
                         //from c in cs.DefaultIfEmpty()
                     where (
                        (m.Recipient.Equals(ownerName, StringComparison.OrdinalIgnoreCase) && m.RecipientType == (int)ownerType && m.Type != (int)MessageType.Sent)
                        ||
                        //Limit sent messages
                        (m.Sender.Equals(ownerName, StringComparison.OrdinalIgnoreCase) && m.SenderType == (int)ownerType && ((type & MessageTypeFlag.Sent) > 0) && m.Type == (int)MessageType.Sent)
                     )
                     select m);

            switch (state)
            {
                case MessageState.Read:
                    q = q.Where(x => x.ReadDate != null);
                    break;

                case MessageState.Unread:
                    q = q.Where(x => x.ReadDate == null);
                    break;
            }

            //filter Message Type
            if (type != MessageTypeFlag.All)
            {
                List<int> messageTypes = new List<int>();

                var flags = Enum.GetValues(typeof(MessageTypeFlag));
                foreach (var flag in flags)
                {
                    //This needs to be cleaned up, we have two enums that are serving a similar purpose
                    var mTFlag = (MessageTypeFlag)flag;
                    if (mTFlag != MessageTypeFlag.All && ((type & mTFlag) > 0))
                    {
                        switch (mTFlag)
                        {
                            case MessageTypeFlag.Sent:
                                messageTypes.Add((int)MessageType.Sent);
                                break;

                            case MessageTypeFlag.Private:
                                messageTypes.Add((int)MessageType.Private);
                                break;

                            case MessageTypeFlag.CommentReply:
                                messageTypes.Add((int)MessageType.CommentReply);
                                break;

                            case MessageTypeFlag.CommentMention:
                                messageTypes.Add((int)MessageType.CommentMention);
                                break;

                            case MessageTypeFlag.SubmissionReply:
                                messageTypes.Add((int)MessageType.SubmissionReply);
                                break;

                            case MessageTypeFlag.SubmissionMention:
                                messageTypes.Add((int)MessageType.SubmissionMention);
                                break;
                        }
                    }
                }
                q = q.Where(x => messageTypes.Contains(x.Type));
            }

            return q;
        }

        private List<int> ConvertMessageTypeFlag(MessageTypeFlag type)
        {
            //filter Message Type
            if (type != MessageTypeFlag.All)
            {
                List<int> messageTypes = new List<int>();

                var flags = Enum.GetValues(typeof(MessageTypeFlag));
                foreach (var flag in flags)
                {
                    //This needs to be cleaned up, we have two enums that are serving a similar purpose
                    var mTFlag = (MessageTypeFlag)flag;
                    if (mTFlag != MessageTypeFlag.All && ((type & mTFlag) > 0))
                    {
                        switch (mTFlag)
                        {
                            case MessageTypeFlag.Sent:
                                messageTypes.Add((int)MessageType.Sent);
                                break;

                            case MessageTypeFlag.Private:
                                messageTypes.Add((int)MessageType.Private);
                                break;

                            case MessageTypeFlag.CommentReply:
                                messageTypes.Add((int)MessageType.CommentReply);
                                break;

                            case MessageTypeFlag.CommentMention:
                                messageTypes.Add((int)MessageType.CommentMention);
                                break;

                            case MessageTypeFlag.SubmissionReply:
                                messageTypes.Add((int)MessageType.SubmissionReply);
                                break;

                            case MessageTypeFlag.SubmissionMention:
                                messageTypes.Add((int)MessageType.SubmissionMention);
                                break;
                        }
                    }
                }
                return messageTypes;
            }
            else
            {
                return null;
            }
        }

        [Authorize]
        public async Task<CommandResponse> DeleteMessages(string ownerName, IdentityType ownerType, MessageTypeFlag type, int? id = null)
        {
            DemandAuthentication();

            //verify if this is a sub request
            if (ownerType == IdentityType.Subverse)
            {
                if (!ModeratorPermission.HasPermission(UserIdentity.UserName, ownerName, ModeratorAction.DeleteMail))
                {
                    return CommandResponse.FromStatus(Status.Denied, "User does not have rights to modify mail");
                }
            }

            //We are going to use this query as the base to form a protective where clause
            var q = GetMessageQueryDapperBase(ownerName, ownerType, type, MessageState.All);
            var d = new DapperDelete();

            //Set the where and parameters from the base query
            d.Where = q.Where;
            d.Parameters = q.Parameters;

            d.Delete = $"m FROM {SqlFormatter.Table("Message", "m")}";

            if (id.HasValue)
            {
                d.Append(x => x.Where, "m.\"ID\" = @ID");
                d.Parameters.Add("ID", id.Value);
            }

            var result = await _db.Connection.ExecuteAsync(d.ToString(), d.Parameters);

            //if (id.HasValue)
            //{
            //    var q = GetMessageQueryBase(ownerName, ownerType, type, MessageState.All);
            //    q = q.Where(x => x.ID == id.Value);
            //    var message = q.FirstOrDefault();

            //    if (message != null)
            //    {
            //        _db.Messages.Remove(message);
            //        await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            //    }
            //}
            //else
            //{
            //    using (var db = new voatEntities())
            //    {
            //        var q = GetMessageQueryBase(db, ownerName, ownerType, type, MessageState.All);
            //        await q.ForEachAsync(x => db.Messages.Remove(x)).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            //        await db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            //    }
            //}

            Task.Run(() => EventNotification.Instance.SendMessageNotice(
                       UserDefinition.Format(ownerName, ownerType),
                       UserDefinition.Format(ownerName, ownerType),
                       type,
                       null,
                       null));

            return CommandResponse.FromStatus(Status.Success, "");
        }

        [Authorize]
        public async Task<CommandResponse> MarkMessages(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state, int? id = null)
        {
            DemandAuthentication();

            //verify if this is a sub request
            if (ownerType == IdentityType.Subverse)
            {
                if (!ModeratorPermission.HasPermission(UserIdentity.UserName, ownerName, ModeratorAction.ReadMail))
                {
                    return CommandResponse.FromStatus(Status.Denied, "User does not have rights to modify mail");
                }
            }

            if (state == MessageState.All)
            {
                return CommandResponse.FromStatus(Status.Ignored, "MessageState must be either Read or Unread");
            }

            var stateToFind = (state == MessageState.Read ? MessageState.Unread : MessageState.Read);
            var setReadDate = new Func<Data.Models.Message, DateTime?>((x) => (state == MessageState.Read ? CurrentDate : (DateTime?)null));

            //We are going to use this query as the base to form a protective where clause
            var q = GetMessageQueryDapperBase(ownerName, ownerType, type, stateToFind);
            var u = new DapperUpdate();

            //Set the where and parameters from the base query
            u.Where = q.Where;
            u.Parameters = q.Parameters;

            u.Update = $"m SET m.\"ReadDate\" = @ReadDate FROM {SqlFormatter.Table("Message", "m")}";

            if (id.HasValue)
            {
                u.Append(x => x.Where, "m.\"ID\" = @ID");
                u.Parameters.Add("ID", id.Value);
            }

            u.Parameters.Add("ReadDate", CurrentDate);

            var result = await _db.Connection.ExecuteAsync(u.ToString(), u.Parameters);

            Task.Run(() => EventNotification.Instance.SendMessageNotice(
                        UserDefinition.Format(ownerName, ownerType),
                        UserDefinition.Format(ownerName, ownerType),
                        type,
                        null,
                        null));

            return CommandResponse.FromStatus(Status.Success, "");
        }

        [Authorize]
        public async Task<MessageCounts> GetMessageCounts(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
        {
            #region Dapper

            using (var db = new VoatDataContext())
            {
                var q = new DapperQuery();
                q.Select = $"SELECT \"Type\", COUNT(*) AS \"Count\" FROM {SqlFormatter.Table("Message")}";
                q.Where = "((\"Recipient\" = @UserName AND \"RecipientType\" = @OwnerType AND \"Type\" <> @SentType) OR (\"Sender\" = @UserName AND \"SenderType\" = @OwnerType AND \"Type\" = @SentType))";

                //Filter
                if (state != MessageState.All)
                {
                    q.Where += String.Format(" AND \"ReadDate\" IS {0} NULL", state == MessageState.Unread ? "" : "NOT");
                }

                var types = ConvertMessageTypeFlag(type);
                if (types != null)
                {
                    q.Where += $" AND \"Type\" {SqlFormatter.In("@MessageTypes")}";
                }

                q.GroupBy = "\"Type\"";
                q.Parameters = new
                {
                    UserName = ownerName,
                    OwnerType = (int)ownerType,
                    SentType = (int)MessageType.Sent,
                    MessageTypes = (types != null ? types.ToArray() : (int[])null)
                }.ToDynamicParameters();

                var results = await db.Connection.QueryAsync<MessageCount>(q.ToString(), q.Parameters);

                var result = new MessageCounts(UserDefinition.Create(ownerName, ownerType));
                result.Counts = results.ToList();
                return result;
            }

            #endregion

            #region EF
            //var q = GetMessageQueryBase(ownerName, ownerType, type, state);
            //var counts = await q.GroupBy(x => x.Type).Select(x => new { x.Key, Count = x.Count() }).ToListAsync();

            //var result = new MessageCounts(UserDefinition.Create(ownerName, ownerType));
            //foreach (var count in counts)
            //{
            //    result.Counts.Add(new MessageCount() { Type = (MessageType)count.Key, Count = count.Count });
            //}
            //return result;
            #endregion


        }

        [Authorize]
        public async Task<IEnumerable<Domain.Models.Message>> GetMessages(MessageTypeFlag type, MessageState state, bool markAsRead = true, SearchOptions options = null)
        {
            return await GetMessages(UserIdentity.UserName, IdentityType.User, type, state, markAsRead, options).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
        }

        [Authorize]
        public async Task<IEnumerable<Domain.Models.Message>> GetMessages(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true, SearchOptions options = null)
        {
            DemandAuthentication();
            if (options == null)
            {
                options = SearchOptions.Default;
            }
            using (var db = new VoatDataContext())
            {
                var q = GetMessageQueryDapperBase(ownerName, ownerType, type, state);
                q.SkipCount = options.Index;
                q.TakeCount = options.Count;

                var messageMap = new Func<Data.Models.Message, Data.Models.Submission, Data.Models.Comment, Domain.Models.Message>((m, s, c) =>
                {
                    var msg = m.Map();
                    msg.Submission = s.Map();
                    msg.Comment = c.Map(m.Subverse);


                    //Set Message Title/Info for API Output
                    switch (msg.Type)
                    {
                        case MessageType.CommentMention:
                        case MessageType.CommentReply:
                        case MessageType.SubmissionReply:
                            if (msg.Comment != null)
                            {
                                msg.Title = msg.Submission?.Title;
                                msg.Content = msg.Comment?.Content;
                                msg.FormattedContent = msg.Comment?.FormattedContent;
                            }
                            else
                            {
                                msg.Title = "Removed";
                                msg.Content = "Removed";
                                msg.FormattedContent = "Removed";
                            }
                            break;
                        case MessageType.SubmissionMention:
                            if (msg.Submission != null)
                            {
                                msg.Title = msg.Submission?.Title;
                                msg.Content = msg.Submission?.Content;
                                msg.FormattedContent = msg.Submission?.FormattedContent;
                            }
                            else
                            {
                                msg.Title = "Removed";
                                msg.Content = "Removed";
                                msg.FormattedContent = "Removed";
                            }
                            break;
                    }

                    return msg;
                });

                var messages = await _db.Connection.QueryAsync<Data.Models.Message, Data.Models.Submission, Data.Models.Comment, Domain.Models.Message>(q.ToString(), messageMap, q.Parameters, splitOn: "ID");

                //mark as read
                if (markAsRead && messages.Any(x => x.ReadDate == null))
                {
                    var update = new DapperUpdate();
                    //Postgres Port: This db is crazy, according to docs UPDATE supports an alias but I can not get it working if I alias the columns
                    update.Update = SqlFormatter.UpdateSetBlock("\"ReadDate\" = @CurrentDate", SqlFormatter.Table("Message"), "m");
                    update.Where = $"\"ReadDate\" IS NULL AND \"ID\" {SqlFormatter.In("@IDs")}";
                    update.Parameters.Add("CurrentDate", Repository.CurrentDate);
                    update.Parameters.Add("IDs", messages.Where(x => x.ReadDate == null).Select(x => x.ID).ToArray());

                    await _db.Connection.ExecuteAsync(update.ToString(), update.Parameters);

                    //await q.Where(x => x.ReadDate == null).ForEachAsync<Models.Message>(x => x.ReadDate = CurrentDate).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    //await db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    Task.Run(() => EventNotification.Instance.SendMessageNotice(
                       UserDefinition.Format(ownerName, ownerType),
                       UserDefinition.Format(ownerName, ownerType),
                       type,
                       null,
                       null));

                }
                return messages;
            }
        }

        #endregion UserMessages

        #region User Related Functions

        public IEnumerable<VoteValue> UserCommentVotesBySubmission(string userName, int submissionID)
        {
            IEnumerable<VoteValue> result = null;
            var q = new DapperQuery();

            q.Select = $"SELECT v.\"CommentID\" AS \"ID\", {SqlFormatter.IsNull("v.\"VoteStatus\"", "0")} AS \"Value\" FROM {SqlFormatter.Table("CommentVoteTracker", "v", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} ON v.\"CommentID\" = c.\"ID\"";
            q.Where = "v.\"UserName\" = @UserName AND c.\"SubmissionID\" = @ID";

            result = _db.Connection.Query<VoteValue>(q.ToString(), new { UserName = userName, ID = submissionID });

            return result;

            //List<CommentVoteTracker> vCache = new List<CommentVoteTracker>();

            //if (!String.IsNullOrEmpty(userName))
            //{
            //    vCache = (from cv in _db.CommentVoteTrackers
            //              join c in _db.Comments on cv.CommentID equals c.ID
            //              where c.SubmissionID == submissionID && cv.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
            //              select cv).ToList();
            //}
            //return vCache;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        public IEnumerable<CommentSaveTracker> UserCommentSavedBySubmission(int submissionID, string userName)
        {
            List<CommentSaveTracker> vCache = new List<CommentSaveTracker>();

            if (!String.IsNullOrEmpty(userName))
            {
                vCache = (from cv in _db.CommentSaveTracker
                          join c in _db.Comment on cv.CommentID equals c.ID
                          where c.SubmissionID == submissionID && cv.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                          select cv).ToList();
            }
            return vCache;
        }

        public IEnumerable<DomainReference> GetSubscriptions(string userName)
        {

            var d = new DapperQuery();
            d.Select = $"SELECT 1 AS \"Type\", s.\"Name\", NULL AS \"OwnerName\" FROM{SqlFormatter.Table("SubverseSet", "subSet")} INNER JOIN {SqlFormatter.Table("SubverseSetList", "setList")} ON subSet.\"ID\" = setList.\"SubverseSetID\" INNER JOIN {SqlFormatter.Table("Subverse", "s")} ON setList.\"SubverseID\" = s.\"ID\" WHERE subSet.\"Type\" = @Type AND subSet.\"Name\" = @SetName AND subSet.\"UserName\" = @UserName ";
            d.Select += $"UNION ALL ";
            d.Select += $"SELECT 2 AS \"Type\", subSet.\"Name\", subSet.\"UserName\" AS \"OwnerName\" FROM {SqlFormatter.Table("SubverseSetSubscription", "setSub")} INNER JOIN {SqlFormatter.Table("SubverseSet", "subSet")} ON subSet.\"ID\" = setSub.\"SubverseSetID\" WHERE setSub.\"UserName\" = @UserName ";
            d.Parameters = new DynamicParameters(new { UserName = userName, Type = (int)SetType.Front, SetName = SetType.Front.ToString() });

            var results = _db.Connection.Query<DomainReference>(d.ToString(), d.Parameters);

            ////TODO: Set Change - needs to retrun subs in user Front set instead.
            //var subs = (from x in _db.SubverseSets
            //            join y in _db.SubverseSetLists on x.ID equals y.SubverseSetID
            //            join s in _db.Subverses on y.SubverseID equals s.ID
            //            where x.Name == SetType.Front.ToString() && x.UserName == userName && x.Type == (int)SetType.Front
            //            select new DomainReference() { Name = s.Name, Type = DomainType.Subverse }
            //            ).ToList();


            ////var subs = (from x in _db.SubverseSubscriptions
            ////            where x.UserName == userName
            ////            select new DomainReference() { Name = x.Subverse, Type = DomainType.Subverse }
            ////            ).ToList();

            ////var sets = (from x in _db.UserSetSubscriptions
            ////            where x.UserName == userName
            ////            select new DomainReference() { Name = x.UserSet.Name, Type = DomainType.Set }).ToList();

            ////subs.AddRange(sets);


            return results;
        }

        public IList<BlockedItem> GetBlockedUsers(string userName)
        {
            var blocked = (from x in _db.UserBlockedUser
                           where x.UserName == userName
                           select new BlockedItem() { Name = x.BlockUser, Type = DomainType.User, CreationDate = x.CreationDate }).ToList();
            return blocked;
        }

        //SET: Backwards Compat
        public async Task<IEnumerable<BlockedItem>> GetBlockedSubverses(string userName)
        {

            var setList = await GetSetListDescription(SetType.Blocked.ToString(), userName, null);
            var blocked = setList.Select(x => new BlockedItem()
            {
                Name = x.Name,
                Type = DomainType.Subverse,
                CreationDate = x.CreationDate
            }).ToList();

            //var blocked = (from x in _db.UserBlockedSubverses
            //               where x.UserName == userName
            //               select new BlockedItem() {
            //                   Name = x.Subverse,
            //                   Type = DomainType.Subverse,
            //                   CreationDate = x.CreationDate
            //               }).ToList();
            return blocked;
        }

        public async Task<UserInformation> GetUserInformation(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName) || userName.TrimSafe().IsEqual("deleted"))
            {
                return null;
            }
            //THIS COULD BE A SOURCE OF BLOCKING
            var q = new QueryUserRecord(userName, CachePolicy.None); //Turn off cache retrieval for this
            var userRecord = await q.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            if (userRecord == null)
            {
                //not a valid user
                //throw new VoatNotFoundException("Can not find user record for " + userName);
                return null;
            }

            userName = userRecord.UserName;

            var userInfo = new UserInformation();
            userInfo.UserName = userRecord.UserName;
            userInfo.RegistrationDate = userRecord.RegistrationDateTime;

            Task<Score>[] tasks = { Task<Score>.Factory.StartNew(() => UserContributionPoints(userName, ContentType.Comment, null, true)),
                                    Task<Score>.Factory.StartNew(() => UserContributionPoints(userName, ContentType.Submission, null, true)),
                                    Task<Score>.Factory.StartNew(() => UserContributionPoints(userName, ContentType.Submission, null, false)),
                                    Task<Score>.Factory.StartNew(() => UserContributionPoints(userName, ContentType.Comment, null, false)),
            };

            var userPreferences = await GetUserPreferences(userName).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            //var pq = new QueryUserPreferences(userName);
            //var userPreferences = await pq.ExecuteAsync();
            //var userPreferences = await GetUserPreferences(userName);

            userInfo.Bio = String.IsNullOrWhiteSpace(userPreferences.Bio) ? STRINGS.DEFAULT_BIO : userPreferences.Bio;
            userInfo.ProfilePicture = VoatPathHelper.AvatarPath(userName, userPreferences.Avatar, true, true, !String.IsNullOrEmpty(userPreferences.Avatar));

            //Task.WaitAll(tasks);
            await Task.WhenAll(tasks).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            userInfo.CommentPoints = tasks[0].Result;
            userInfo.SubmissionPoints = tasks[1].Result;
            userInfo.SubmissionVoting = tasks[2].Result;
            userInfo.CommentVoting = tasks[3].Result;

            //Old Sequential
            //userInfo.CommentPoints = UserContributionPoints(userName, ContentType.Comment);
            //userInfo.SubmissionPoints = UserContributionPoints(userName, ContentType.Submission);
            //userInfo.SubmissionVoting = UserVotingBehavior(userName, ContentType.Submission);
            //userInfo.CommentVoting = UserVotingBehavior(userName, ContentType.Comment);

            //Badges
            var userBadges = await (from b in _db.Badge
                                    join ub in _db.UserBadge on b.ID equals ub.BadgeID into ubn
                                    from uball in ubn.DefaultIfEmpty()
                                    where
                                    uball.UserName == userName

                                    //(virtual badges)
                                    ||
                                    (b.ID == "whoaverse" && (userInfo.RegistrationDate < new DateTime(2015, 1, 2)))
                                    ||
                                    (b.ID == "alphauser" && (userInfo.RegistrationDate > new DateTime(2015, 1, 2) && userInfo.RegistrationDate < new DateTime(2016, 10, 10)))
                                    ||
                                    (b.ID == "betauser" && userInfo.RegistrationDate > (new DateTime(2016, 10, 10)))
                                    ||
                                    (b.ID == "cakeday" && userInfo.RegistrationDate.Year < CurrentDate.Year && userInfo.RegistrationDate.Month == CurrentDate.Month && userInfo.RegistrationDate.Day == CurrentDate.Day)
                                    select new Voat.Domain.Models.UserBadge()
                                    {
                                        CreationDate = (uball == null ? userInfo.RegistrationDate : uball.CreationDate),
                                        Name = b.Name,
                                        Title = b.Title,
                                        Graphic = b.Graphic,
                                    }
                              ).ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            userInfo.Badges = userBadges;

            return userInfo;
        }

        public async Task<Models.UserPreference> GetUserPreferences(string userName)
        {
            Models.UserPreference result = null;
            if (!String.IsNullOrEmpty(userName))
            {
                var query = _db.UserPreference.Where(x => (x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                result = await query.FirstOrDefaultAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            }

            if (result == null)
            {
                result = new Data.Models.UserPreference();
                Repository.SetDefaultUserPreferences(result);
                result.UserName = userName;
            }

            return result;
        }

        public Score UserVotingBehavior(string userName, ContentType contentType = ContentType.Comment | ContentType.Submission, TimeSpan? timeSpan = null)
        {
            Score vb = UserContributionPoints(userName, contentType, null, false, timeSpan);

            //if ((type & ContentType.Comment) > 0)
            //{
            //    var c = GetUserVotingBehavior(userName, ContentType.Comment, span);
            //    vb.Combine(c);
            //}
            //if ((type & ContentType.Submission) > 0)
            //{
            //    var c = GetUserVotingBehavior(userName, ContentType.Submission, span);
            //    vb.Combine(c);
            //}

            return vb;
        }
        [Obsolete("User UserContributionPoints instead when implemented", true)]
        private Score GetUserVotingBehavior(string userName, ContentType type, TimeSpan? span = null)
        {
            var score = new Score();
            using (var db = new VoatDataContext())
            {
                DateTime? compareDate = null;
                if (span.HasValue)
                {
                    compareDate = CurrentDate.Subtract(span.Value);
                }

                var cmd = db.Connection.CreateCommand();
                cmd.CommandText = $"SELECT x.\"VoteStatus\", ABS({SqlFormatter.IsNull("SUM(x.\"VoteStatus\")", "0")}) AS \"Count\" FROM {SqlFormatter.Table(type == ContentType.Comment ? "CommentVoteTracker" : "SubmissionVoteTracker", "x", null, "NOLOCK")} WHERE x.\"UserName\" = @UserName AND (x.\"CreationDate\" >= @CompareDate OR @CompareDate IS NULL) GROUP BY x.\"VoteStatus\"";
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

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
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
            }
            return score;
        }
        public int UserVoteStatus(string userName, ContentType type, int id)
        {
            var result = UserVoteStatus(userName, type, new int[] { id });
            if (result.Any())
            {
                return result.First().Value;
            }
            return 0;
        }
        public IEnumerable<VoteValue> UserVoteStatus(string userName, ContentType type, int[] id)
        {
            IEnumerable<VoteValue> result = null;
            var q = new DapperQuery();

            switch (type)
            {
                case ContentType.Comment:
                    q.Select = $"SELECT \"CommentID\" AS \"ID\", {SqlFormatter.IsNull("\"VoteStatus\"", "0")} AS \"Value\" FROM {SqlFormatter.Table("CommentVoteTracker", null, null, "NOLOCK")}";
                    q.Where = $"\"UserName\" = @UserName AND \"CommentID\" {SqlFormatter.In("@ID")}";
                    break;
                case ContentType.Submission:
                    q.Select = $"SELECT \"SubmissionID\" AS \"ID\", {SqlFormatter.IsNull("\"VoteStatus\"", "0")} AS \"Value\" FROM {SqlFormatter.Table("SubmissionVoteTracker", null, null, "NOLOCK")}";
                    q.Where = $"\"UserName\" = @UserName AND \"SubmissionID\" {SqlFormatter.In("@ID")}";
                    break;
            }

            result = _db.Connection.Query<VoteValue>(q.ToString(), new { UserName = userName, ID = id });

            return result;
        }
        public int UserCommentCount(string userName, TimeSpan? span, string subverse = null)
        {
            DateTime? compareDate = null;
            if (span.HasValue)
            {
                compareDate = CurrentDate.Subtract(span.Value);
            }

            var result = (from x in _db.Comment
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
            var q = new DapperQuery();
            q.Select = $"COUNT(*) FROM {SqlFormatter.Table("Submission", null, null, "NOLOCK")}";
            q.Where = "\"UserName\" = @UserName";
            if (compareDate != null)
            {
                q.Append(x => x.Where, "\"CreationDate\" >= @StartDate");
            }
            if (type != null)
            {
                q.Append(x => x.Where, "\"Type\" = @Type");
            }
            if (!String.IsNullOrEmpty(subverse))
            {
                q.Append(x => x.Where, "\"Subverse\" = @Subverse");
            }

            var count = _db.Connection.ExecuteScalar<int>(q.ToString(), new { UserName = userName, StartDate = compareDate, Type = type, Subverse = subverse });

            //Logic was buggy here
            //var result = (from x in _db.Submissions
            //              where
            //                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
            //                &&
            //                ((x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
            //                && (compareDate.HasValue && x.CreationDate >= compareDate)
            //                && (type != null && x.Type == (int)type.Value) || type == null)
            //              select x).Count();
            //return result;

            return count;
        }
        public Score UserContributionPoints(string userName, ContentType contentType, string subverse = null, bool isReceived = true, TimeSpan? timeSpan = null)
        {

            Func<IEnumerable<dynamic>, Score> processRecords = new Func<IEnumerable<dynamic>, Score>(records =>
            {
                Score score = new Score();
                if (records != null && records.Any())
                {
                    foreach (var record in records)
                    {
                        if (record.VoteStatus == 1)
                        {
                            score.UpCount = isReceived ? (int)record.VoteValue : (int)record.VoteCount;
                        }
                        else if (record.VoteStatus == -1)
                        {
                            score.DownCount = isReceived ? (int)record.VoteValue : (int)record.VoteCount;
                        }
                    }
                }
                return score;
            });

            var groupingClause = $"SELECT \"UserName\", \"IsReceived\", \"ContentType\", \"VoteStatus\", SUM(\"VoteCount\") AS \"VoteCount\", SUM(\"VoteValue\") AS \"VoteValue\" FROM ({"{0}"}) AS a GROUP BY a.\"UserName\", a.\"IsReceived\", a.\"ContentType\", a.\"VoteStatus\"";

            var archivedPointsClause = $"SELECT \"UserName\", \"IsReceived\", \"ContentType\", \"VoteStatus\", \"VoteCount\", \"VoteValue\" FROM {SqlFormatter.Table("UserContribution", "uc", null, "NOLOCK")} WHERE uc.\"UserName\" = @UserName AND uc.\"IsReceived\" = @IsReceived AND uc.\"ContentType\" = @ContentType UNION ALL ";
            var alias = "";
            DateTime? dateRange = timeSpan.HasValue ? CurrentDate.Subtract(timeSpan.Value) : (DateTime?)null;
            Score s = new Score();
            using (var db = new VoatDataContext())
            {
                var contentTypes = contentType.GetEnumFlags();
                foreach (var contentTypeToQuery in contentTypes)
                {
                    var q = new DapperQuery();

                    switch (contentTypeToQuery)
                    {
                        case ContentType.Comment:

                            //basic point calc query
                            q.Select = $"SELECT @UserName AS \"UserName\", @IsReceived AS \"IsReceived\", @ContentType AS \"ContentType\", v.\"VoteStatus\" AS \"VoteStatus\", 1 AS \"VoteCount\", ABS(v.\"VoteValue\") AS \"VoteValue\" FROM {SqlFormatter.Table("CommentVoteTracker", "v", null, "NOLOCK")} ";
                            q.Select += $"INNER JOIN {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} ON c.\"ID\" = v.\"CommentID\" INNER JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON s.\"ID\" = c.\"SubmissionID\""; 

                            //This controls whether we search for given or received votes
                            alias = (isReceived ? "c" : "v");
                            q.Append(x => x.Where, $"{alias}.\"UserName\" = @UserName");

                            break;
                        case ContentType.Submission:
                            //basic point calc query
                            q.Select = $"SELECT @UserName AS \"UserName\", @IsReceived AS \"IsReceived\", @ContentType AS \"ContentType\", v.\"VoteStatus\" AS \"VoteStatus\", 1 AS \"VoteCount\", ABS(v.\"VoteValue\") AS \"VoteValue\" FROM {SqlFormatter.Table("SubmissionVoteTracker", "v", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON s.\"ID\" = v.\"SubmissionID\"";

                            //This controls whether we search for given or received votes
                            alias = (isReceived ? "s" : "v");
                            q.Append(x => x.Where, $"{alias}.\"UserName\" = @UserName");

                            break;
                        default:
                            throw new NotImplementedException($"Type {contentType.ToString()} is not supported");
                    }

                    //if subverse/daterange calc we do not use archived table
                    if (!String.IsNullOrEmpty(subverse) || dateRange.HasValue)
                    {
                        if (!String.IsNullOrEmpty(subverse))
                        {
                            q.Append(x => x.Where, "s.\"Subverse\" = @Subverse");
                        }
                        if (dateRange.HasValue)
                        {
                            q.Append(x => x.Where, "v.\"CreationDate\" >= @DateRange");
                        }
                    }
                    else
                    {
                        q.Select = archivedPointsClause + q.Select;
                        q.Append(x => x.Where, "s.\"ArchiveDate\" IS NULL");
                    }

                    string statement = String.Format(groupingClause, q.ToString());
                    System.Diagnostics.Debug.WriteLine("Query Output");
                    System.Diagnostics.Debug.WriteLine(statement);
                    var records = db.Connection.Query(statement, new
                    {
                        UserName = userName,
                        IsReceived = isReceived,
                        Subverse = subverse,
                        ContentType = (int)contentType,
                        DateRange = dateRange
                    });
                    Score result = processRecords(records);
                    s.Combine(result);
                }
            }
            return s;
        }

        //public Score UserContributionPoints_OLD(string userName, ContentType type, string subverse = null)
        //{
        //    Score s = new Score();
        //    using (var db = new voatEntities())
        //    {
        //        if ((type & ContentType.Comment) > 0)
        //        {
        //            var cmd = db.Connection.CreateCommand();
        //            cmd.CommandText = @"SELECT 'UpCount' = CAST(ABS(ISNULL(SUM(c.UpCount),0)) AS INT), 'DownCount' = CAST(ABS(ISNULL(SUM(c.DownCount),0)) AS INT) FROM Comment c WITH (NOLOCK)
        //                            INNER JOIN Submission s WITH (NOLOCK) ON(c.SubmissionID = s.ID)
        //                            WHERE c.UserName = @UserName
        //                            AND (s.Subverse = @Subverse OR @Subverse IS NULL)
        //                            AND c.IsAnonymized = 0"; //this prevents anon votes from showing up in stats
        //            cmd.CommandType = System.Data.CommandType.Text;

        //            var param = cmd.CreateParameter();
        //            param.ParameterName = "UserName";
        //            param.DbType = System.Data.DbType.String;
        //            param.Value = userName;
        //            cmd.Parameters.Add(param);

        //            param = cmd.CreateParameter();
        //            param.ParameterName = "Subverse";
        //            param.DbType = System.Data.DbType.String;
        //            param.Value = String.IsNullOrEmpty(subverse) ? (object)DBNull.Value : subverse;
        //            cmd.Parameters.Add(param);

        //            if (cmd.Connection.State != System.Data.ConnectionState.Open)
        //            {
        //                cmd.Connection.Open();
        //            }
        //            using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
        //            {
        //                if (reader.Read())
        //                {
        //                    s.Combine(new Score() { UpCount = (int)reader["UpCount"], DownCount = (int)reader["DownCount"] });
        //                }
        //            }
        //        }

        //        if ((type & ContentType.Submission) > 0)
        //        {
        //            var cmd = db.Connection.CreateCommand();
        //            cmd.CommandText = @"SELECT 
        //                            'UpCount' = CAST(ABS(ISNULL(SUM(s.UpCount), 0)) AS INT), 
        //                            'DownCount' = CAST(ABS(ISNULL(SUM(s.DownCount), 0)) AS INT) 
        //                            FROM Submission s WITH (NOLOCK)
        //                            WHERE s.UserName = @UserName
        //                            AND (s.Subverse = @Subverse OR @Subverse IS NULL)
        //                            AND s.IsAnonymized = 0";

        //            var param = cmd.CreateParameter();
        //            param.ParameterName = "UserName";
        //            param.DbType = System.Data.DbType.String;
        //            param.Value = userName;
        //            cmd.Parameters.Add(param);

        //            param = cmd.CreateParameter();
        //            param.ParameterName = "Subverse";
        //            param.DbType = System.Data.DbType.String;
        //            param.Value = String.IsNullOrEmpty(subverse) ? (object)DBNull.Value : subverse;
        //            cmd.Parameters.Add(param);

        //            if (cmd.Connection.State != System.Data.ConnectionState.Open)
        //            {
        //                cmd.Connection.Open();
        //            }
        //            using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
        //            {
        //                if (reader.Read())
        //                {
        //                    s.Combine(new Score() { UpCount = (int)reader["UpCount"], DownCount = (int)reader["DownCount"] });
        //                }
        //            }
        //        }
        //    }
        //    return s;
        //}

        [Authorize]
        public async Task<CommandResponse<bool?>> SubscribeUser(DomainReference domainReference, SubscriptionAction action)
        {
            DemandAuthentication();

            CommandResponse<bool?> response = new CommandResponse<bool?>(null, Status.NotProcessed, "");

            switch (domainReference.Type)
            {
                case DomainType.Subverse:

                    var subverse = GetSubverseInfo(domainReference.Name);
                    if (subverse == null)
                    {
                        return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Subverse does not exist");
                    }
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Subverse is disabled");
                    }

                    var set = GetOrCreateSubverseSet(new SubverseSet() { Name = SetType.Front.ToString(), UserName = UserIdentity.UserName, Type = (int)SetType.Front, Description = "Front Page Subverse Subscriptions" });

                    response = await SetSubverseListChange(set, subverse, action);


                    //var countChanged = false;

                    //if (action == SubscriptionAction.Subscribe)
                    //{
                    //    if (!_db.SubverseSubscriptions.Any(x => x.Subverse.Equals(domainReference.Name, StringComparison.OrdinalIgnoreCase) && x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase)))
                    //    {
                    //        var sub = new SubverseSubscription { UserName = UserIdentity.UserName, Subverse = domainReference.Name };
                    //        _db.SubverseSubscriptions.Add(sub);
                    //        countChanged = true;
                    //    }
                    //}
                    //else
                    //{
                    //    var sub = _db.SubverseSubscriptions.FirstOrDefault(x => x.Subverse.Equals(domainReference.Name, StringComparison.OrdinalIgnoreCase) && x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase));
                    //    if (sub != null)
                    //    {
                    //        _db.SubverseSubscriptions.Remove(sub);
                    //        countChanged = true;
                    //    }
                    //}

                    //await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    //if (countChanged)
                    //{
                    //    await UpdateSubverseSubscriberCount(domainReference, action).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    //}

                    break;
                case DomainType.Set:
                    var setb = GetSet(domainReference.Name, domainReference.OwnerName);
                    if (setb == null)
                    {
                        return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Set does not exist");
                    }

                    var subscribeAction = SubscriptionAction.Toggle;

                    var setSubscriptionRecord = _db.SubverseSetSubscription.FirstOrDefault(x => x.SubverseSetID == setb.ID && x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase));

                    if (setSubscriptionRecord == null && ((action == SubscriptionAction.Subscribe) || action == SubscriptionAction.Toggle))
                    {
                        var sub = new SubverseSetSubscription { UserName = UserIdentity.UserName, SubverseSetID = setb.ID, CreationDate = CurrentDate };
                        _db.SubverseSetSubscription.Add(sub);
                        subscribeAction = SubscriptionAction.Subscribe;
                        response.Response = true;


                        //db.SubverseSetLists.Add(new SubverseSetList { SubverseSetID = set.ID, SubverseID = subverse.ID, CreationDate = CurrentDate });
                        //response.Response = true;
                    }
                    else if (setSubscriptionRecord != null && ((action == SubscriptionAction.Unsubscribe) || action == SubscriptionAction.Toggle))
                    {
                        _db.SubverseSetSubscription.Remove(setSubscriptionRecord);
                        subscribeAction = SubscriptionAction.Unsubscribe;
                        response.Response = false;

                        //db.SubverseSetLists.Remove(setSubverseRecord);
                        //response.Response = false;
                    }



                    //if (action == SubscriptionAction.Subscribe)
                    //{
                    //    if (!_db.SubverseSetSubscriptions.Any(x => x.SubverseSetID == setb.ID && x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase)))
                    //    {
                    //        var sub = new SubverseSetSubscription { UserName = UserIdentity.UserName, SubverseSetID = setb.ID };
                    //        _db.SubverseSetSubscriptions.Add(sub);
                    //        countChanged = true;
                    //        response.Response = true;
                    //    }
                    //}
                    //else
                    //{
                    //    var sub = _db.SubverseSetSubscriptions.FirstOrDefault(x => x.SubverseSetID == setb.ID && x.UserName.Equals(UserIdentity.UserName, StringComparison.OrdinalIgnoreCase));
                    //    if (sub != null)
                    //    {
                    //        _db.SubverseSetSubscriptions.Remove(sub);
                    //        countChanged = true;
                    //        response.Response = false;
                    //    }
                    //}

                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    if (subscribeAction != SubscriptionAction.Toggle)
                    {
                        await UpdateSubverseSubscriberCount(domainReference, subscribeAction).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    }
                    response.Status = Status.Success;

                    break;
                default:
                    throw new NotImplementedException(String.Format("{0} subscriptions not implemented yet", domainReference.Type));
                    break;
            }
            return response;
        }

        private async Task UpdateSubverseSubscriberCount(DomainReference domainReference, SubscriptionAction action)
        {
            //TODO: This logic is jacked because of the action has been extended to include a toggle value thus this needs refactoring
            if (action != SubscriptionAction.Toggle)
            {

                int incrementValue = action == SubscriptionAction.Subscribe ? 1 : -1;
                var u = new DapperUpdate();

                switch (domainReference.Type)
                {
                    case DomainType.Subverse:
                        //Postgre Port
                        //u.Update = $"UPDATE s SET \"SubscriberCount\" = ({SqlFormatter.IsNull("\"SubscriberCount\"", "0")} + @IncrementValue) FROM {SqlFormatter.Table("Subverse", "s")}";
                        u.Update = $"{SqlFormatter.UpdateSetBlock($"\"SubscriberCount\" = ({SqlFormatter.IsNull("\"SubscriberCount\"", "0")} + @IncrementValue)", SqlFormatter.Table("Subverse", null), "s")}";

                        u.Where = "s.\"Name\" = @Name";
                        u.Parameters = new DynamicParameters(new { Name = domainReference.Name, IncrementValue = incrementValue });

                        break;
                    case DomainType.Set:

                        //Postgre Port 
                        //u.Update = $"UPDATE s SET \"SubscriberCount\" = ({SqlFormatter.IsNull("\"SubscriberCount\"", "0")} + @IncrementValue) FROM {SqlFormatter.Table("SubverseSet", "s")}";
                        u.Update = $"{SqlFormatter.UpdateSetBlock($"\"SubscriberCount\" = ({SqlFormatter.IsNull("\"SubscriberCount\"", "0")} + @IncrementValue)", SqlFormatter.Table("SubverseSet", null), "s")}";

                        u.Where = "s.\"Name\" = @Name";

                        if (!String.IsNullOrEmpty(domainReference.OwnerName))
                        {
                            u.Append(x => x.Where, "s.\"UserName\" = @OwnerName");
                        }
                        else
                        {
                            u.Append(x => x.Where, "s.\"UserName\" IS NULL");
                        }
                        u.Parameters = new DynamicParameters(new { Name = domainReference.Name, IncrementValue = incrementValue, OwnerName = domainReference.OwnerName });
                        break;
                    case DomainType.User:
                        throw new NotImplementedException("User subscriber count not implemented");
                        break;
                }
                var count = await _db.Connection.ExecuteAsync(u.ToString(), u.Parameters);
            }
        }

        public async Task<CommandResponse<bool?>> BanUserFromSubverse(string userName, string subverse, string reason, bool? force = null)
        {
            bool? status = null;

            //check perms
            if (!ModeratorPermission.HasPermission(UserIdentity.UserName, subverse, Domain.Models.ModeratorAction.Banning))
            {
                return new CommandResponse<bool?>(status, Status.Denied, "User does not have permission to ban");
            }

            userName = userName.TrimSafe();

            string originalUserName = UserHelper.OriginalUsername(userName);

            // prevent bans if user doesn't exist
            if (String.IsNullOrEmpty(originalUserName))
            {
                return new CommandResponse<bool?>(status, Status.Denied, "User can not be found? Are you at the right site?");
            }

            // get model for selected subverse
            var subverseModel = GetSubverseInfo(subverse);

            if (subverseModel == null)
            {
                return new CommandResponse<bool?>(status, Status.Denied, "Subverse can not be found");
            }

            // check that user is not already banned in given subverse
            var existingBan = _db.SubverseBan.FirstOrDefault(a => a.UserName == originalUserName && a.Subverse == subverseModel.Name);

            if (existingBan != null && (force.HasValue && force.Value))
            {
                return new CommandResponse<bool?>(status, Status.Denied, "User is currently banned. You can't reban.");
            }

            //Force logic:
            //True = ennsure ban
            //False = ensure remove ban
            //Null = toggle ban
            bool? addBan = (force.HasValue ?
                                (force.Value ?
                                    (existingBan == null ? true : (bool?)null) :
                                    (existingBan == null ? (bool?)null : false))
                            : !(existingBan == null));

            if (addBan.HasValue)
            {
                if (addBan.Value)
                {
                    if (String.IsNullOrWhiteSpace(reason))
                    {
                        return new CommandResponse<bool?>(status, Status.Denied, "Banning a user requires a reason to be given");
                    }
                    // prevent bans of the current user
                    if (UserIdentity.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CommandResponse<bool?>(status, Status.Denied, "Can not ban yourself or a blackhole appears");
                    }
                    //check if user is mod for "The benning"
                    if (ModeratorPermission.IsModerator(originalUserName, subverseModel.Name))
                    {
                        return new CommandResponse<bool?>(status, Status.Denied, "Moderators of subverse can not be banned. Is this a coup attempt?");
                    }
                    status = true; //added ban
                    var subverseBan = new Data.Models.SubverseBan();
                    subverseBan.UserName = originalUserName;
                    subverseBan.Subverse = subverseModel.Name;
                    subverseBan.CreatedBy = UserIdentity.UserName;
                    subverseBan.CreationDate = Repository.CurrentDate;
                    subverseBan.Reason = reason;
                    _db.SubverseBan.Add(subverseBan);
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                }
                else
                {
                    status = false; //removed ban
                    _db.SubverseBan.Remove(existingBan);
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                }
            }

            if (status.HasValue)
            {
                var msg = new SendMessage();
                msg.Sender = $"v/{subverseModel.Name}";
                msg.Recipient = originalUserName;

                if (status.Value)
                {
                    //send ban msg
                    msg.Subject = $"You've been banned from v/{subverse} :(";
                    msg.Message = $"@{UserIdentity.UserName} has banned you from v/{subverseModel.Name} for the following reason: *{reason}*";
                }
                else
                {
                    //send unban msg
                    msg.Subject = $"You've been unbanned from v/{subverse} :)";
                    msg.Message = $"@{UserIdentity.UserName} has unbanned you from v/{subverseModel.Name}. Play nice. Promise me. Ok, I believe you.";
                }
                SendMessage(msg);
            }
            return new CommandResponse<bool?>(status, Status.Success, "");
        }

        #endregion User Related Functions

        #region ModLog

        public async Task<IEnumerable<Domain.Models.SubverseBan>> GetModLogBannedUsers(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                var data = (from b in db.SubverseBan
                            where b.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                            select new Domain.Models.SubverseBan
                            {
                                CreatedBy = b.CreatedBy,
                                CreationDate = b.CreationDate,
                                Reason = b.Reason,
                                Subverse = b.Subverse,
                                ID = b.ID,
                                UserName = b.UserName
                            });
                data = data.OrderByDescending(x => x.CreationDate).Skip(options.Index).Take(options.Count);
                var results = await data.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return results;
            }
        }
        public async Task<IEnumerable<Data.Models.SubmissionRemovalLog>> GetModLogRemovedSubmissions(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                db.EnableCacheableOutput();

                var data = (from b in db.SubmissionRemovalLog
                            join s in db.Submission on b.SubmissionID equals s.ID
                            where s.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                            select b).Include(x => x.Submission);

                var data2 = data.OrderByDescending(x => x.CreationDate).Skip(options.Index).Take(options.Count);
                var results = await data2.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return results;
            }
        }
        public async Task<IEnumerable<Domain.Models.CommentRemovalLog>> GetModLogRemovedComments(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                db.EnableCacheableOutput();

                var data = (from b in db.CommentRemovalLog
                            join c in db.Comment on b.CommentID equals c.ID
                            join s in db.Submission on c.SubmissionID equals s.ID
                            where s.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                            select b).Include(x => x.Comment).Include(x => x.Comment.Submission);

                var data_ordered = data.OrderByDescending(x => x.CreationDate).Skip(options.Index).Take(options.Count);
                var results = await data_ordered.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                //TODO: Move to DomainMaps
                var mapToDomain = new Func<Data.Models.CommentRemovalLog, Domain.Models.CommentRemovalLog>(d =>
                {
                    var m = new Domain.Models.CommentRemovalLog();
                    m.CreatedBy = d.Moderator;
                    m.Reason = d.Reason;
                    m.CreationDate = d.CreationDate;

                    m.Comment = new SubmissionComment();
                    m.Comment.ID = d.Comment.ID;
                    m.Comment.UpCount = (int)d.Comment.UpCount;
                    m.Comment.DownCount = (int)d.Comment.DownCount;
                    m.Comment.Content = d.Comment.Content;
                    m.Comment.FormattedContent = d.Comment.FormattedContent;
                    m.Comment.IsDeleted = d.Comment.IsDeleted;
                    m.Comment.CreationDate = d.Comment.CreationDate;

                    m.Comment.IsAnonymized = d.Comment.IsAnonymized;
                    m.Comment.UserName = m.Comment.IsAnonymized ? d.Comment.ID.ToString() : d.Comment.UserName;
                    m.Comment.LastEditDate = d.Comment.LastEditDate;
                    m.Comment.ParentID = d.Comment.ParentID;
                    m.Comment.Subverse = d.Comment.Submission.Subverse;
                    m.Comment.SubmissionID = d.Comment.SubmissionID;

                    m.Comment.Submission.Title = d.Comment.Submission.Title;
                    m.Comment.Submission.IsAnonymized = d.Comment.Submission.IsAnonymized;
                    m.Comment.Submission.UserName = m.Comment.Submission.IsAnonymized ? d.Comment.Submission.ID.ToString() : d.Comment.Submission.UserName;
                    m.Comment.Submission.IsDeleted = d.Comment.Submission.IsDeleted;

                    return m;
                });

                var mapped = results.Select(mapToDomain).ToList();

                return mapped;
            }
        }



        #endregion

        #region Moderator Functions

        public async Task<CommandResponse<RemoveModeratorResponse>> RemoveModerator(int subverseModeratorRecordID, bool allowSelfRemovals)
        {
            DemandAuthentication();

            var response = new RemoveModeratorResponse();
            var originUserName = UserIdentity.UserName;

            // get moderator name for selected subverse
            var subModerator = await _db.SubverseModerator.FindAsync(subverseModeratorRecordID).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            if (subModerator == null)
            {
                return new CommandResponse<RemoveModeratorResponse>(response, Status.Invalid, "Can not find record");
            }

            //Set response data
            response.SubverseModerator = subModerator;
            response.OriginUserName = originUserName;
            response.TargetUserName = subModerator.UserName;
            response.Subverse = subModerator.Subverse;

            var subverse = GetSubverseInfo(subModerator.Subverse);
            if (subverse == null)
            {
                return new CommandResponse<RemoveModeratorResponse>(response, Status.Invalid, "Can not find subverse");
            }

            // check if caller has clearance to remove a moderator
            if (!ModeratorPermission.HasPermission(originUserName, subverse.Name, Domain.Models.ModeratorAction.RemoveMods))
            {
                return new CommandResponse<RemoveModeratorResponse>(response, Status.Denied, "User doesn't have permissions to execute action");
            }

            var allowRemoval = false;
            var errorMessage = "Rules do not allow removal";

            if (allowSelfRemovals && originUserName.Equals(subModerator.UserName, StringComparison.OrdinalIgnoreCase))
            {
                allowRemoval = true;
            }
            else if (subModerator.UserName.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                allowRemoval = false;
                errorMessage = "System moderators can not be removed or they get sad";
            }
            else
            {
                //Determine if removal is allowed:
                //Logic:
                //L1: Can remove L1's but only if they invited them / or they were added after them
                var currentModLevel = ModeratorPermission.Level(originUserName, subverse.Name).Value; //safe to get value as previous check ensures is mod
                var targetModLevel = (ModeratorLevel)subModerator.Power;

                switch (currentModLevel)
                {
                    case ModeratorLevel.Owner:
                        if (targetModLevel == ModeratorLevel.Owner)
                        {
                            var isTargetOriginalMod = (String.IsNullOrEmpty(subModerator.CreatedBy) && !subModerator.CreationDate.HasValue); //Currently original mods have these fields nulled
                            if (isTargetOriginalMod)
                            {
                                allowRemoval = false;
                                errorMessage = "The creator can not be destroyed";
                            }
                            else
                            {
                                //find current mods record
                                var originModeratorRecord = _db.SubverseModerator.FirstOrDefault(x =>
                                    x.Subverse.Equals(subModerator.Subverse, StringComparison.OrdinalIgnoreCase)
                                    && x.UserName.Equals(originUserName, StringComparison.OrdinalIgnoreCase));

                                //Creators of subs have no creation date so set it low
                                var originModCreationDate = (originModeratorRecord.CreationDate.HasValue ? originModeratorRecord.CreationDate.Value : new DateTime(2000, 1, 1));

                                if (originModeratorRecord == null)
                                {
                                    allowRemoval = false;
                                    errorMessage = "Can not find current mod record";
                                }
                                else
                                {
                                    allowRemoval = (originModCreationDate < subModerator.CreationDate);
                                    errorMessage = "Moderator has seniority. Oldtimers can't be removed by a young'un";
                                }
                            }
                        }
                        else
                        {
                            allowRemoval = true;
                        }
                        break;

                    default:
                        allowRemoval = (targetModLevel > currentModLevel);
                        errorMessage = "Only moderators at a lower level can be removed";
                        break;
                }
            }

            //ensure mods can only remove mods that are a lower level than themselves
            if (allowRemoval)
            {
                // execute removal
                _db.SubverseModerator.Remove(subModerator);
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                ////clear mod cache
                //CacheHandler.Instance.Remove(CachingKey.SubverseModerators(subverse.Name));

                return new CommandResponse<RemoveModeratorResponse>(response, Status.Success, String.Empty);
            }
            else
            {
                return new CommandResponse<RemoveModeratorResponse>(response, Status.Denied, errorMessage);
            }
        }

        #endregion Moderator Functions

        #region RuleReports

        public async Task<IEnumerable<Data.Models.RuleSet>> GetRuleSets(string subverse, ContentType? contentType)
        {
            var typeFilter = contentType == ContentType.Submission ? "r.SubmissionID IS NOT NULL" : "r.CommentID IS NOT NULL";
            var q = new DapperQuery();

            q.Select = $"* FROM {SqlFormatter.Table("RuleSet", "r")}";
            q.Where = $"(r.\"Subverse\" = @Subverse OR r.\"Subverse\" IS NULL) AND (r.\"ContentType\" = @ContentType OR r.\"ContentType\" IS NULL) AND r.\"IsActive\" = {SqlFormatter.BooleanLiteral(true)}";
            q.OrderBy = "r.\"SortOrder\" ASC";

            int? intContentType = contentType == null ? (int?)null : (int)contentType;

            var data = await _db.Connection.QueryAsync<Data.Models.RuleSet>(q.ToString(), new { Subverse = subverse, ContentType = intContentType });

            return data;

        }
        public async Task<Dictionary<ContentItem, IEnumerable<ContentUserReport>>> GetRuleReports(string subverse, ContentType? contentType = null, int hours = 24, ReviewStatus reviewedStatus = ReviewStatus.Unreviewed, int[] ruleSetID = null)
        {
            var q = new DapperQuery();

            q.Select = $"SELECT rr.\"Subverse\", rr.\"UserName\", rr.\"SubmissionID\", rr.\"CommentID\", rr.\"RuleSetID\", r.\"Name\", r.\"Description\", COUNT(*) AS \"Count\", MAX(rr.\"CreationDate\") AS \"MostRecent\" FROM {SqlFormatter.Table("RuleReport", "rr", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("RuleSet", "r", null, "NOLOCK")} ON rr.\"RuleSetID\" = r.\"ID\"";

            //--LEFT JOIN Submission s WITH (NOLOCK) ON s.ID = rr.SubmissionID
            //--LEFT JOIN Comment c WITH (NOLOCK) ON c.ID = rr.CommentID
            //WHERE
            //    (rr.Subverse = @Subverse OR @Subverse IS NULL)
            //    AND
            //    (rr.CreationDate >= @StartDate OR @StartDate IS NULL)
            //    AND
            //    (rr.CreationDate <= @EndDate OR @EndDate IS NULL)
            //    AND {typeFilter}
            q.Where = "(rr.\"Subverse\" = @Subverse OR @Subverse IS NULL) AND (rr.\"CreationDate\" >= @StartDate OR @StartDate IS NULL) AND (rr.\"CreationDate\" <= @EndDate OR @EndDate IS NULL)";
            q.OrderBy = "\"MostRecent\" DESC";

            if (contentType != null)
            {
                q.Append(x => x.Where, contentType == ContentType.Submission ? "rr.\"SubmissionID\" IS NOT NULL" : "rr.\"CommentID\" IS NOT NULL");
            }

            if (reviewedStatus != ReviewStatus.Any)
            {
                q.Append(x => x.Where, reviewedStatus == ReviewStatus.Reviewed ? "rr.\"ReviewedDate\" IS NOT NULL" : "rr.\"ReviewedDate\" IS NULL");
            }

            if (ruleSetID != null && ruleSetID.Any())
            {
                q.Append(x => x.Where, $"rr.\"RuleSetID\" {SqlFormatter.In("@RuleSetID")}");
            }

            q.GroupBy = "rr.\"Subverse\", rr.\"UserName\", rr.\"SubmissionID\", rr.\"CommentID\", rr.\"RuleSetID\", r.\"Name\", r.\"Description\"";

            DateTime? startDate = Repository.CurrentDate.AddHours(hours * -1);
            DateTime? endDate = null;

            var data = await _db.Connection.QueryAsync<ContentUserReport>(q.ToString(), new { Subverse = subverse, StartDate = startDate, EndDate = endDate, RuleSetID = ruleSetID });

            Dictionary<ContentItem, IEnumerable<ContentUserReport>> groupedData = new Dictionary<ContentItem, IEnumerable<ContentUserReport>>();

            //load target content and add to output dictionary
            if (contentType == null || contentType == ContentType.Submission)
            {
                var ids = data.Where(x => x.SubmissionID != null && x.CommentID == null).Select(x => x.SubmissionID.Value).Distinct();
                //Get associated content
                var submissions = await GetSubmissions(ids.ToArray());
                var dict = ids.ToDictionary(x => new ContentItem() { Submission = DomainMaps.Map(submissions.FirstOrDefault(s => s.ID == x)), ContentType = ContentType.Submission }, x => data.Where(y => y.SubmissionID.Value == x && !y.CommentID.HasValue));
                dict.ToList().ForEach(x => groupedData.Add(x.Key, x.Value));
            }

            if (contentType == null || contentType == ContentType.Comment)
            {
                var ids = data.Where(x => x.SubmissionID != null && x.CommentID != null).Select(x => x.CommentID.Value).Distinct();
                //Get associated content
                var comments = await GetComments(ids.ToArray());
                var dict = ids.ToDictionary(x => new ContentItem() { Comment = comments.FirstOrDefault(s => s.ID == x), ContentType = ContentType.Comment }, x => data.Where(y => y.CommentID.HasValue && y.CommentID.Value == x));
                dict.ToList().ForEach(x => groupedData.Add(x.Key, x.Value));
            }

            return groupedData;

        }
        [Authorize]
        public async Task<CommandResponse> MarkReportsAsReviewed(string subverse, ContentType contentType, int id)
        {

            DemandAuthentication();

            if (!SubverseExists(subverse))
            {
                return CommandResponse.FromStatus(Status.Invalid, "Subverse does not exist");
            }
            if (!ModeratorPermission.HasPermission(UserIdentity.UserName, subverse, ModeratorAction.MarkReports))
            {
                return CommandResponse.FromStatus(Status.Denied, "User does not have permissions to mark reports");
            }

            var q = new DapperUpdate();
            q.Update = $"r SET r.\"ReviewedBy\" = @UserName, r.\"ReviewedDate\" = @CreationDate FROM {SqlFormatter.Table("RuleReport", "r")}";
            if (contentType == ContentType.Submission)
            {
                q.Where = "r.\"Subverse\" = @Subverse AND r.\"SubmissionID\" = @ID";
            }
            else
            {
                q.Where = "r.\"Subverse\" = @Subverse AND r.\"CommentID\" = @ID";
            }
            q.Append(x => x.Where, "r.\"ReviewedDate\" IS NULL AND r.\"ReviewedBy\" IS NULL");

            var result = await _db.Connection.ExecuteAsync(q.ToString(), new { Subverse = subverse, ID = id, UserName = UserIdentity.UserName, CreationDate = CurrentDate });

            return CommandResponse.FromStatus(Status.Success);

        }

        [Authorize]
        public async Task<CommandResponse> SaveRuleReport(ContentType contentType, int id, int ruleID)
        {
            DemandAuthentication();

            var duplicateFilter = "";
            switch (contentType)
            {
                case ContentType.Comment:
                    duplicateFilter = "AND ruleExists.\"CommentID\" = @ID";
                    break;
                case ContentType.Submission:
                    duplicateFilter = "AND ruleExists.\"SubmissionID\" = @ID AND ruleExists.\"CommentID\" IS NULL";
                    break;
                default:
                    throw new NotImplementedException("ContentType not supported");
                    break;
            }

            var existsClause = $"SELECT * FROM {SqlFormatter.Table("RuleReport", "ruleExists")} WHERE ruleExists.\"CreatedBy\" = @UserName {duplicateFilter}";

            var body = $"INSERT INTO {SqlFormatter.Table("RuleReport")} (\"Subverse\", \"UserName\", \"SubmissionID\", \"CommentID\", \"RuleSetID\", \"CreatedBy\", \"CreationDate\") ";

            switch (contentType)
            {
                case ContentType.Comment:
                    body += $"SELECT s.\"Subverse\", NULL, s.\"ID\", c.\"ID\", @RuleID, @UserName, @Date FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("Comment", "c", null, "NOLOCK")} ON c.\"SubmissionID\" = s.\"ID\" INNER JOIN {SqlFormatter.Table("RuleSet", "r", null, "NOLOCK")} ON r.\"ID\" = @RuleID AND (r.\"Subverse\" = s.\"Subverse\" OR r.\"Subverse\" IS NULL) AND (r.\"ContentType\" = @ContentType OR r.\"ContentType\" IS NULL) WHERE c.\"ID\" = @ID AND c.\"IsDeleted\" = {SqlFormatter.BooleanLiteral(false)} AND r.\"IsActive\" = {SqlFormatter.BooleanLiteral(true)}";
                    break;
                case ContentType.Submission:
                    body += $"SELECT s.\"Subverse\", NULL, s.\"ID\", NULL, @RuleID, @UserName, @Date FROM {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} INNER JOIN {SqlFormatter.Table("RuleSet", "r", null, "NOLOCK")} ON r.\"ID\" = @RuleID AND (r.\"Subverse\" = s.\"Subverse\" OR r.\"Subverse\" IS NULL) AND (r.\"ContentType\" = @ContentType OR r.\"ContentType\" IS NULL) WHERE s.\"ID\" = @ID AND s.\"IsDeleted\" = {SqlFormatter.BooleanLiteral(false)} AND r.\"IsActive\" = {SqlFormatter.BooleanLiteral(true)}";
                    break;
            }
            //filter out banned users
            body += $" AND NOT EXISTS (SELECT * FROM {SqlFormatter.Table("BannedUser")} WHERE \"UserName\" = @UserName) AND NOT EXISTS (SELECT * FROM {SqlFormatter.Table("SubverseBan")} WHERE \"UserName\" = @UserName AND \"Subverse\" = s.\"Subverse\")";
            body += $" AND NOT EXISTS ({existsClause})";

            //var statement = SqlFormatter.IfExists(false, existsClause, body, null);

           var result = await _db.Connection.ExecuteAsync(body, new { UserName = UserIdentity.UserName, ID = id, RuleID = ruleID, ContentType = (int)contentType, Date = CurrentDate });

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
        //    log.UserName = UserIdentity.UserName;
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

        #endregion Admin Functions

        #region Block

        /// <summary>
        /// Unblocks a domain type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        public async Task Unblock(DomainType domainType, string name)
        {
            await Block(domainType, name, SubscriptionAction.Unsubscribe);
        }

        /// <summary>
        /// Blocks a domain type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        public async Task Block(DomainType domainType, string name)
        {
            await Block(domainType, name, SubscriptionAction.Subscribe);
        }

        /// <summary>
        /// Blocks, Unblocks, or Toggles blocks
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="name"></param>
        /// <param name="block">If null then toggles, else, blocks or unblocks based on value</param>
        public async Task<CommandResponse<bool?>> Block(DomainType domainType, string name, SubscriptionAction action)
        {
            DemandAuthentication();

            var response = new CommandResponse<bool?>();

            switch (domainType)
            {
                case DomainType.Subverse:

                    var exists = _db.Subverse.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (exists == null)
                    {
                        throw new VoatNotFoundException("Subverse '{0}' does not exist", name);
                    }
                    //Add to user Block Set
                    //Set propercased name
                    name = exists.Name;

                    var set = GetOrCreateSubverseSet(new SubverseSet() { Name = SetType.Blocked.ToString(), UserName = UserIdentity.UserName, Type = (int)SetType.Blocked, Description = "Blocked Subverses" });
                    //var action = block == null ? (SubscriptionAction?)null : (block.Value ? SubscriptionAction.Subscribe : SubscriptionAction.Unsubscribe);

                    response = await SetSubverseListChange(set, exists, action);

                    //var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.Subverse.ToLower() == name.ToLower() && n.UserName == UserIdentity.UserName);
                    //if (subverseBlock == null && ((block.HasValue && block.Value) || !block.HasValue))
                    //{
                    //    db.UserBlockedSubverses.Add(new UserBlockedSubverse { UserName = UserIdentity.UserName, Subverse = name, CreationDate = Repository.CurrentDate });
                    //    response.Response = true;
                    //}
                    //else if (subverseBlock != null && ((block.HasValue && !block.Value) || !block.HasValue))
                    //{
                    //    db.UserBlockedSubverses.Remove(subverseBlock);
                    //    response.Response = false;
                    //}
                    //db.SaveChanges();
                    break;

                case DomainType.User:

                    //Ensure user exists, get propercased user name
                    name = UserHelper.OriginalUsername(name);
                    if (String.IsNullOrEmpty(name))
                    {
                        return new CommandResponse<bool?>(null, Status.Error, "User does not exist");
                    }
                    if (UserIdentity.UserName.IsEqual(name))
                    {
                        return new CommandResponse<bool?>(null, Status.Error, "Attempting to open worm hold denied. Can not block yourself.");
                    }

                    var userBlock = _db.UserBlockedUser.FirstOrDefault(n => n.BlockUser.ToLower() == name.ToLower() && n.UserName == UserIdentity.UserName);
                    if (userBlock == null && (action == SubscriptionAction.Subscribe || action == SubscriptionAction.Toggle))
                    {
                        _db.UserBlockedUser.Add(new UserBlockedUser { UserName = UserIdentity.UserName, BlockUser = name, CreationDate = Repository.CurrentDate });
                        response.Response = true;
                    }
                    else if (userBlock != null && (action == SubscriptionAction.Unsubscribe || action == SubscriptionAction.Toggle))
                    {
                        _db.UserBlockedUser.Remove(userBlock);
                        response.Response = false;
                    }

                    await _db.SaveChangesAsync();
                    break;

                default:
                    throw new NotImplementedException(String.Format("Blocking of {0} is not implemented yet", domainType.ToString()));
                    break;
            }

            response.Status = Status.Success;
            return response;
        }

        #endregion Block

        #region Misc


        private async Task<string> GetRandomSubverse(bool nsfw, bool restrict = true)
        {

            var q = new DapperQuery();
            q.Select = $"SELECT TOP 1 s.\"Name\" FROM {SqlFormatter.Table("Subverse", "s")} INNER JOIN {SqlFormatter.Table("Submission", "sm")} ON s.\"Name\" = sm.\"Subverse\"";
            q.Where = $"s.\"Name\" != 'all' AND s.IsAdult = @IsAdult AND s.\"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)}";
            q.GroupBy = "s.\"Name\"";
            q.OrderBy = "NEWID()";
            q.Parameters = new DynamicParameters(new { IsAdult = nsfw, HourLimit = (24 * 7) });

            if (restrict)
            {
                q.Append(x => x.Where, "s.\"SubscriberCount\" > 10");
                //Postgre Port 
                //q.Having = "DATEDIFF(HH, MAX(sm.\"CreationDate\"), GETUTCDATE()) < @HourLimit";
                q.Having = "MAX(sm.\"CreationDate\") >= @EndDate";
                q.Parameters.Add("EndDate", CurrentDate.AddHours(-24));
            }

            return await _db.Connection.ExecuteScalarAsync<string>(q.ToString(), q.Parameters);
            /*
            SELECT TOP 1 s.Name FROM Subverse s
            INNER JOIN Submission sm ON s.Name = sm.Subverse
            WHERE 
            s.SubscriberCount > 10
            AND s.Name != 'all'
            AND s.IsAdult = @IsAdult
            AND s.IsAdminDisabled = 0
            GROUP BY s.Name
            HAVING DATEDIFF(HH, MAX(sm.CreationDate), GETUTCDATE()) < (24 * 7)
            ORDER BY NEWID()
            */

        }

        public async Task<string> GetRandomSubverse(bool nsfw)
        {
            var sub = await GetRandomSubverse(nsfw, true);
            if (String.IsNullOrEmpty(sub))
            {
                sub = await GetRandomSubverse(nsfw, false);
            }
            return sub;
        }
        public double? HighestRankInSubverse(string subverse)
        {
            var q = new DapperQuery();
            q.Select = $"MAX({SqlFormatter.IsNull("\"Rank\"", "0")}) FROM {SqlFormatter.Table("Submission", null, null, "NOLOCK")}";
            q.Where = "\"Subverse\" = @Subverse AND \"ArchiveDate\" IS NULL";
            //q.OrderBy = "\"Rank\" DESC";
            q.Parameters = new { Subverse = subverse }.ToDynamicParameters();

            var result = _db.Connection.ExecuteScalar<double?>(q.ToString(), q.Parameters);
            return result;

            //using (var db = new voatEntities())
            //{
            //    var submission = db.Submissions.OrderByDescending(x => x.Rank).Where(x => x.Subverse == subverse).FirstOrDefault();
            //    if (submission != null)
            //    {
            //        return submission.Rank;
            //    }
            //    return null;
            //}
        }

        public int VoteCount(string sourceUser, string targetUser, ContentType contentType, Vote voteType, TimeSpan timeSpan)
        {
            var sum = 0;
            var startDate = CurrentDate.Subtract(timeSpan);

            if ((contentType & ContentType.Comment) > 0)
            {
                var count = (from x in _db.CommentVoteTracker
                             join c in _db.Comment on x.CommentID equals c.ID
                             where
                                 x.UserName.Equals(sourceUser, StringComparison.OrdinalIgnoreCase)
                                 &&
                                 c.UserName.Equals(targetUser, StringComparison.OrdinalIgnoreCase)
                                 &&
                                 x.CreationDate > startDate
                                 &&
                                 (voteType == Vote.None || x.VoteStatus == (int)voteType)
                             select x).Count();
                sum += count;
            }
            if ((contentType & ContentType.Submission) > 0)
            {
                var count = (from x in _db.SubmissionVoteTracker
                             join s in _db.Submission on x.SubmissionID equals s.ID
                             where
                                 x.UserName.Equals(sourceUser, StringComparison.OrdinalIgnoreCase)
                                 &&
                                 s.UserName.Equals(targetUser, StringComparison.OrdinalIgnoreCase)
                                 &&
                                 x.CreationDate > startDate
                                 &&
                                 (voteType == Vote.None || x.VoteStatus == (int)voteType)
                             select x).Count();
                sum += count;
            }
            return sum;
        }

        public bool HasAddressVoted(string addressHash, ContentType contentType, int id)
        {
            var result = true;
            switch (contentType)
            {
                case ContentType.Comment:
                    result = _db.CommentVoteTracker.Any(x => x.CommentID == id && x.IPAddress == addressHash);
                    break;

                case ContentType.Submission:
                    result = _db.SubmissionVoteTracker.Any(x => x.SubmissionID == id && x.IPAddress == addressHash);
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

                keywords.ForEach(x =>
                {
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
                case SortAlgorithm.Relative:
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

        private static IQueryable<Domain.Models.SubmissionComment> ApplyCommentSearch(SearchOptions options, IQueryable<Domain.Models.SubmissionComment> query)
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

                keywords.ForEach(x =>
                {
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
            return (from x in _db.BannedDomain
                    orderby x.CreationDate descending
                    select x).ToList();
        }
        public IEnumerable<BannedDomain> BannedDomains(string[] domains, int? gtldMinimumPartEvaulationCount = 1)
        {

            List<string> alldomains = domains.Where(x => !String.IsNullOrEmpty(x)).ToList();

            if (alldomains.Any())
            {
                if (gtldMinimumPartEvaulationCount != null)
                {
                    int minPartCount = Math.Max(1, gtldMinimumPartEvaulationCount.Value);

                    foreach (var domain in domains)
                    {
                        var pieces = domain.Split('.');
                        if (pieces.Length > minPartCount)
                        {
                            pieces = pieces.Reverse().ToArray();
                            for (int i = pieces.Length - 1; i >= minPartCount; i--)
                            {
                                string newDomain = String.Join(".", pieces.Take(i).Reverse());
                                if (!String.IsNullOrEmpty(newDomain))
                                {
                                    alldomains.Add(newDomain);
                                }
                            }
                        }
                    }
                }

                var q = new DapperQuery();
                q.Select = $"* FROM {SqlFormatter.Table("BannedDomain")}";
                q.Where = $"\"Domain\" {SqlFormatter.In("@Domains")}";
                q.Parameters = new { Domains = alldomains.ToArray() }.ToDynamicParameters();

                var bannedDomains = _db.Connection.Query<BannedDomain>(q.ToString(), q.Parameters);
                return bannedDomains;
            }

            //return empty
            return new List<BannedDomain>();
        }
        public string SubverseForComment(int commentID)
        {
            var subname = (from x in _db.Comment
                           where x.ID == commentID
                           select x.Submission.Subverse).FirstOrDefault();
            return subname;
        }

        //private IPrincipal User
        //{
        //    get
        //    {
        //        return UserIdentity.Principal;
        //    }
        //}

        public bool SubverseExists(string subverse)
        {
            return _db.Subverse.Any(x => x.Name == subverse);
        }

        public string ToCorrectSubverseCasing(string subverse)
        {
            if (!String.IsNullOrEmpty(subverse))
            {
                var sub = _db.Subverse.FirstOrDefault(x => x.Name == subverse);
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
            //CORE_PORT: Loosing User Thread Context
            if (UserIdentity.Principal == null)
            {
                throw new VoatSecurityException("CorePort: User context not available");
            }
            if (!UserIdentity.IsAuthenticated || String.IsNullOrEmpty(UserIdentity.UserName))
            {
                throw new VoatSecurityException("Current process not authenticated");
            }
            if (UserDefinition.Parse(UserIdentity.UserName) == null)
            {
                throw new VoatSecurityException("Invalid user identity detected");
            }
        }

        //TODO: Make async
        public Models.EventLog Log(EventLog log)
        {
            var newLog = _db.EventLog.Add(log);
            _db.SaveChanges();
            return newLog.Entity;
        }

        public static DateTime CurrentDate
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public IEnumerable<Data.Models.Filter> GetFilters(bool activeOnly = true)
        {
            var q = new DapperQuery();
            q.Select = $"* FROM {SqlFormatter.Table("Filter")}";
            if (activeOnly)
            {
                q.Where = "\"IsActive\" = @IsActive";
            }
            //will return empty list I believe, so should be runtime cacheable 
            return _db.Connection.Query<Data.Models.Filter>(q.ToString(), new { IsActive = activeOnly });
        }

        protected CommandResponse<T> MapRuleOutCome<T>(RuleOutcome outcome, T result)
        {
            switch (outcome.Result)
            {
                case RuleResult.Denied:
                    return CommandResponse.FromStatus(result, Status.Denied, outcome.Message);

                default:
                    return CommandResponse.Successful(result);
            }
        }

        public FeaturedDomainReferenceDetails GetFeatured()
        {
            //This is for lazy admins, if you don't change the featured item this will cut it off
            var dayCutOff = 7;

            var d = new DapperQuery();
            d.Select += $"SELECT d.\"Type\", d.\"ID\", \"Name\", {SqlFormatter.IsNull("f.\"Title\"", "d.\"Title\"")} AS \"Title\", {SqlFormatter.IsNull("f.\"Description\"", "d.\"Description\"")} AS \"Description\", d.\"SubscriberCount\", d.\"OwnerName\", d.\"CreationDate\", f.\"StartDate\" AS \"FeaturedDate\", f.\"CreatedBy\" AS \"FeaturedBy\" FROM {SqlFormatter.Table("Featured", "f")} ";
            d.Select += $"INNER JOIN ( ";
            d.Select += $"SELECT 1 AS \"Type\", \"ID\", \"Name\", \"Title\", \"Description\", \"CreationDate\", \"SubscriberCount\", \"CreatedBy\" AS \"OwnerName\" FROM {SqlFormatter.Table("Subverse")} WHERE \"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)} ";
            d.Select += $"UNION ";
            d.Select += $"SELECT 2 AS \"Type\", \"ID\", \"Name\", \"Title\", \"Description\", \"CreationDate\", \"SubscriberCount\", \"UserName\" AS \"OwnerName\" FROM {SqlFormatter.Table("SubverseSet")} WHERE \"IsPublic\" = {SqlFormatter.BooleanLiteral(true)} ";
            d.Select += $") AS D ON D.\"ID\" = f.\"DomainID\" AND D.\"Type\" = f.\"DomainType\" ";

            d.Where = "f.\"StartDate\" <= @CurrentDate AND (f.\"EndDate\" >= @CurrentDate OR f.\"EndDate\" IS NULL )";

            if (dayCutOff > 0)
            {
                //Postgre Port
                //d.Append(x => x.Where, "(f.\"EndDate\" IS NOT NULL OR (f.\"EndDate\" IS NULL AND DATEDIFF(HH, f.\"StartDate\", GETUTCDATE()) <= @Hours))");
                //d.Parameters.Add("Hours", (dayCutOff * 24));
                d.Append(x => x.Where, "(f.\"EndDate\" IS NOT NULL OR (f.\"EndDate\" IS NULL AND f.\"StartDate\" >= @EndDate))");
                d.Parameters.Add("EndDate", Repository.CurrentDate.AddHours(dayCutOff * 24 * -1));
            }

            d.OrderBy = "f.\"StartDate\" DESC";
            d.Parameters.Add("CurrentDate", CurrentDate); //I really have no idea why we are passing in a current time. In fact, it is both pointless and error prone, but for some reason, deep inside me, I cannot change this. True, it would take me all of 4 seconds, but this isn’t the time investment. It is something deeper, something unexplainable. I feel that somehow, for some reason, this will save us in the future. I shall leave it in order to save the future people! I am legend?

            var result = _db.Connection.QueryFirstOrDefault<FeaturedDomainReferenceDetails>(d.ToString(), d.Parameters);

            return result;
        }


        #endregion Misc

        #region Search 

        public async Task<IEnumerable<SubverseSubmissionSetting>> SubverseSubmissionSettingsSearch(string subverseName, bool exactMatch)
        {

            var q = new DapperQuery();
            q.Select = $"\"Name\", \"IsAnonymized\", \"IsAdult\" FROM {SqlFormatter.Table("Subverse")}";
            q.OrderBy = "\"SubscriberCount\" DESC, \"CreationDate\" ASC";

            if (exactMatch)
            {
                q.Where = "\"Name\" = @Name";
                q.TakeCount = 1;
            }
            else
            {
                q.Where = "\"Name\" LIKE CONCAT(@Name, '%') OR \"Name\" = @Name";
                q.TakeCount = 10;
            }

            return await _db.Connection.QueryAsync<SubverseSubmissionSetting>(q.ToString(), new { Name = subverseName });

        }

        #endregion

        #region User

        public async Task<CommandResponse> DeleteAccount(DeleteAccountOptions options)
        {
            DemandAuthentication();

            if (!options.UserName.IsEqual(options.ConfirmUserName))
            {
                return CommandResponse.FromStatus(Status.Error, "Confirmation UserName does not match");
            }

            if (UserIdentity.UserName.IsEqual(options.UserName))
            {

                var userName = UserIdentity.UserName;

                //ensure banned user blocked from operation
                if (_db.BannedUser.Any(x => x.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase)))
                {
                    return CommandResponse.FromStatus(Status.Denied, "User is Globally Banned");
                }

                using (var userManager = VoatUserManager.Create())
                {
                    var userAccount = await userManager.FindAsync(userName, options.CurrentPassword);
                    if (userAccount != null)
                    {

                        //Verify Email before proceeding
                        var setRecoveryEmail = !String.IsNullOrEmpty(options.RecoveryEmailAddress) && options.RecoveryEmailAddress.IsEqual(options.ConfirmRecoveryEmailAddress);
                        if (setRecoveryEmail)
                        {
                            var userWithEmail = await userManager.FindByEmailAsync(options.RecoveryEmailAddress);
                            if (userWithEmail != null && userWithEmail.UserName != userAccount.UserName)
                            {
                                return CommandResponse.FromStatus(Status.Error, "This email address is in use, please provide a unique address");
                            }
                        }

                        List<DapperBase> statements = new List<DapperBase>();
                        var deleteText = "Account Deleted By User";
                        //Comments
                        switch (options.Comments.Value)
                        {
                            case DeleteOption.Anonymize:
                                var a = new DapperUpdate();
                                a.Update = SqlFormatter.UpdateSetBlock($"\"IsAnonymized\" = {SqlFormatter.BooleanLiteral(true)}", SqlFormatter.Table("Comment")); 
                                a.Where = "\"UserName\" = @UserName";
                                a.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(a);
                                break;
                            case DeleteOption.Delete:
                                var d = new DapperUpdate();
                                d.Update = SqlFormatter.UpdateSetBlock($"\"IsDeleted\" = {SqlFormatter.BooleanLiteral(true)}, \"Content\" = '{deleteText}'", SqlFormatter.Table("Comment"));
                                d.Where = "\"UserName\" = @UserName";
                                d.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(d);
                                break;
                        }
                        //Text Submissions
                        switch (options.TextSubmissions.Value)
                        {
                            case DeleteOption.Anonymize:
                                var a = new DapperUpdate();
                                a.Update = SqlFormatter.UpdateSetBlock($"\"IsAnonymized\" = {SqlFormatter.BooleanLiteral(true)}", SqlFormatter.Table("Submission"));
                                a.Where = $"\"UserName\" = @UserName AND \"Type\" = {(int)SubmissionType.Text}";
                                a.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(a);
                                break;
                            case DeleteOption.Delete:
                                var d = new DapperUpdate();
                                d.Update = SqlFormatter.UpdateSetBlock($"\"IsDeleted\" = {SqlFormatter.BooleanLiteral(true)}, \"Title\" = '{deleteText}', \"Content\" = '{deleteText}'", SqlFormatter.Table("Submission"));
                                d.Where = $"\"UserName\" = @UserName AND \"Type\" = {(int)SubmissionType.Text}";
                                d.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(d);
                                break;
                        }
                        //Link Submissions
                        switch (options.LinkSubmissions.Value)
                        {
                            case DeleteOption.Anonymize:
                                var a = new DapperUpdate();
                                a.Update = SqlFormatter.UpdateSetBlock($"\"IsAnonymized\" = {SqlFormatter.BooleanLiteral(true)}", SqlFormatter.Table("Submission"));
                                a.Where = $"\"UserName\" = @UserName AND \"Type\" = {(int)SubmissionType.Link}";
                                a.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(a);
                                break;
                            case DeleteOption.Delete:
                                var d = new DapperUpdate();
                                d.Update = SqlFormatter.UpdateSetBlock($"\"IsDeleted\" = {SqlFormatter.BooleanLiteral(true)}, \"Title\" = '{deleteText}', \"Url\" = 'https://{Settings.SiteDomain}'", SqlFormatter.Table("Submission"));
                                d.Where = $"\"UserName\" = @UserName AND \"Type\" = {(int)SubmissionType.Link}";
                                d.Parameters = new DynamicParameters(new { UserName = userName });
                                statements.Add(d);
                                break;
                        }

                        // resign from all moderating positions
                        _db.SubverseModerator.RemoveRange(_db.SubverseModerator.Where(m => m.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase)));
                        var u = new DapperDelete();
                        u.Delete = SqlFormatter.DeleteBlock(SqlFormatter.Table("SubverseModerator"));
                        u.Where = "\"UserName\" = @UserName";
                        u.Parameters = new DynamicParameters(new { UserName = userName });
                        statements.Add(u);

                        //Messages
                        u = new DapperDelete();
                        u.Delete = SqlFormatter.DeleteBlock(SqlFormatter.Table("Message"));
                        u.Where = $"((\"Recipient\" = @UserName AND \"RecipientType\" = {(int)IdentityType.User} AND \"Type\" {SqlFormatter.In("@RecipientTypes")}))";
                        u.Parameters = new DynamicParameters(new
                        {
                            UserName = userName,
                            RecipientTypes = new int[] {
                                (int)MessageType.CommentMention,
                                (int)MessageType.CommentReply,
                                (int)MessageType.SubmissionMention,
                                (int)MessageType.SubmissionReply,
                            }
                        });
                        statements.Add(u);

                        //Start Update Tasks
                        //TODO: Run this in better 
                        //var updateTasks = statements.Select(x => Task.Factory.StartNew(() => { _db.Connection.ExecuteAsync(x.ToString(), x.Parameters); }));

                        foreach (var statement in statements)
                        {
                            await _db.Connection.ExecuteAsync(statement.ToString(), statement.Parameters);
                        }


                        // delete user preferences
                        var userPrefs = _db.UserPreference.Find(userName);
                        if (userPrefs != null)
                        {
                            // delete avatar
                            if (userPrefs.Avatar != null)
                            {
                                var avatarFilename = userPrefs.Avatar;
                                if (Settings.UseContentDeliveryNetwork)
                                {
                                    // try to delete from CDN
                                    CloudStorageUtility.DeleteBlob(avatarFilename, "avatars");
                                }
                                else
                                {
                                    // try to remove from local FS - I think this code is retarded
                                    string tempAvatarLocation = Settings.DestinationPathAvatars + '\\' + userName + ".jpg";

                                    // the avatar file was not found at expected path, abort
                                    if (FileSystemUtility.FileExists(tempAvatarLocation, Settings.DestinationPathAvatars))
                                    {
                                        File.Delete(tempAvatarLocation);
                                    }
                                }
                            }


                            var updatePrefStatement = new DapperUpdate();
                            updatePrefStatement.Update = SqlFormatter.UpdateSetBlock("\"Bio\" = NULL, \"Avatar\" = NULL", SqlFormatter.Table("UserPreference"));
                            updatePrefStatement.Where = "\"UserName\" = @UserName";
                            updatePrefStatement.Parameters.Add("UserName", userName);
                            await _db.Connection.ExecuteAsync(updatePrefStatement.ToString(), updatePrefStatement.Parameters);
                        }


                        //// UNDONE: keep this updated as new features are added (delete sets etc)
                        //// username will stay permanently reserved to prevent someone else from registering it and impersonating
                        //await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        try
                        {
                            //Flag Deleted Account
                            var badgeID = (setRecoveryEmail ? "deleted2" : "deleted");

                            //Get rid of EF


                            var statement = "INSERT INTO \"dbo\".\"UserBadge\" (\"UserName\", \"BadgeID\", \"CreationDate\") " +
                                            "SELECT @UserName, @BadgeID, @Date " +
                                            "WHERE NOT EXISTS (SELECT * FROM \"dbo\".\"UserBadge\" WHERE \"UserName\" = @UserName AND \"BadgeID\" = @BadgeID)";
                            await _db.Connection.ExecuteAsync(statement, new { BadgeID = badgeID, UserName = userName, Date = CurrentDate });

                            //var existing = _db.UserBadges.FirstOrDefault(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && x.BadgeID.Equals(badgeID, StringComparison.OrdinalIgnoreCase));
                            //if (existing == null)
                            //{
                            //    _db.UserBadges.Add(new Models.UserBadge() { BadgeID = badgeID, CreationDate = CurrentDate, UserName = userName });
                            //    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                            //}
                        }
                        catch (Exception ex)
                        {
                            /*no-op*/
                            EventLogger.Log(ex);
                        }

                        var userID = userAccount.Id;

                        //Recovery
                        if (setRecoveryEmail)
                        {
                            //Account is recoverable but locked for x days
                            var endLockOutDate = CurrentDate.AddDays(3 * 30);

                            userAccount.Email = options.RecoveryEmailAddress;
                            userAccount.LockoutEnd = endLockOutDate;
                            userAccount.LockoutEnabled = true;

                        }
                        else
                        {
                            userAccount.Email = null;
                            //await userManager.SetEmailAsync(userID, null);
                        }
                        await userManager.UpdateAsync(userAccount).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        //Password
                        string randomPassword = "";
                        using (SHA512 shaM = new SHA512Managed())
                        {
                            randomPassword = Convert.ToBase64String(shaM.ComputeHash(Encoding.UTF8.GetBytes(Path.GetRandomFileName())));
                        }
                        await userManager.ChangePasswordAsync(userAccount, options.CurrentPassword, randomPassword).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                        //log this to ensure delete options working as expected
                        var logEntry = new Logging.LogInformation();
                        logEntry.Type = Logging.LogType.Audit;
                        logEntry.Category = "DeleteAccount";
                        logEntry.UserName = UserIdentity.UserName;
                        logEntry.Message = String.Format("{0} deleted account", UserIdentity.UserName);
                        logEntry.Origin = Settings.Origin.ToString();
                        logEntry.Data = new
                        {
                            userName = options.UserName,
                            reason = options.Reason,
                            comments = options.Comments,
                            textSubmissions = options.TextSubmissions,
                            linkSubmissions = options.LinkSubmissions,
                            recoveryAddress = options.RecoveryEmailAddress
                        };

                        EventLogger.Instance.Log(logEntry);

                        return CommandResponse.FromStatus(Status.Success);
                    }
                }
            }
            // user account could not be found
            return CommandResponse.FromStatus(Status.Error, "User Account Not Found");
        }
        #endregion


    }
}
