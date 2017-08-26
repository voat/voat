using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse<string>> RegenerateThumbnail(int submissionID)
        {
            DemandAuthentication();

            // get model for selected submission
            var submission = _db.Submission.Find(submissionID);
            var response = CommandResponse.FromStatus(Status.Error);

            if (submission == null || submission.IsDeleted)
            {
                return CommandResponse.FromStatus("", Status.Error, "Submission is missing or deleted");
            }
            var subverse = submission.Subverse;

            // check if caller is subverse moderator, if not, deny change
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.AssignFlair))
            {
                return CommandResponse.FromStatus("", Status.Denied, "Moderator Permissions are not satisfied");
            }
            try
            {
                throw new NotImplementedException();

                await _db.SaveChangesAsync();

                return CommandResponse.FromStatus("", Status.Success);
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<CommandResponse<string>>(ex);
            }
        }
        public async Task<CommandResponse<bool>> ToggleNSFW(int submissionID)
        {
            DemandAuthentication();

            // get model for selected submission
            var submission = _db.Submission.Find(submissionID);
            var response = CommandResponse.FromStatus(Status.Error);

            if (submission == null || submission.IsDeleted)
            {
                return CommandResponse.FromStatus(false, Status.Error, "Submission is missing or deleted");
            }
            var subverse = submission.Subverse;

            if (!User.Identity.Name.IsEqual(submission.UserName))
            {
                // check if caller is subverse moderator, if not, deny change
                if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.AssignFlair))
                {
                    return CommandResponse.FromStatus(false, Status.Denied, "Moderator Permissions are not satisfied");
                }
            }
            try
            {
                submission.IsAdult = !submission.IsAdult;
                
                await _db.SaveChangesAsync();

                return CommandResponse.FromStatus(submission.IsAdult, Status.Success);
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<CommandResponse<bool>>(ex);
            }
        }
        public async Task<CommandResponse> ToggleSticky(int submissionID, string subverse = null, bool clearExisting = false, int stickyLimit = 3)
        {
            DemandAuthentication();

            // get model for selected submission
            var submission = _db.Submission.Find(submissionID);
            var response = CommandResponse.FromStatus(Status.Error);


            if (submission == null || submission.IsDeleted)
            {
                return CommandResponse.FromStatus(Status.Error, "Submission is missing or deleted");
            }
            //Eventually we want users to be able to sticky other subs posts, but for now make sure we don't allow this
            subverse = submission.Subverse;

            // check if caller is subverse moderator, if not, deny change
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.AssignStickies))
            {
                return CommandResponse.FromStatus(Status.Denied, "Moderator Permissions are not satisfied");
            }
            int affectedCount = 0;
            try
            {
                // find and clear current sticky if toggling
                var existingSticky = _db.StickiedSubmission.FirstOrDefault(s => s.SubmissionID == submissionID);
                if (existingSticky != null)
                {
                    _db.StickiedSubmission.Remove(existingSticky);
                    affectedCount += -1;
                }
                else
                {
                    if (clearExisting)
                    {
                        // remove all stickies for subverse matching submission subverse
                        _db.StickiedSubmission.RemoveRange(_db.StickiedSubmission.Where(s => s.Subverse == subverse));
                        affectedCount = 0;
                    }

                    // set new submission as sticky
                    var stickyModel = new Data.Models.StickiedSubmission
                    {
                        SubmissionID = submissionID,
                        CreatedBy = User.Identity.Name,
                        CreationDate = Repository.CurrentDate,
                        Subverse = subverse
                    };

                    _db.StickiedSubmission.Add(stickyModel);
                    affectedCount += 1;
                }

                //limit sticky counts 
                var currentCount = _db.StickiedSubmission.Count(x => x.Subverse == subverse);
                if ((currentCount + affectedCount) > stickyLimit)
                {
                    return CommandResponse.FromStatus(Status.Denied, $"Stickies are limited to {stickyLimit}");
                }

                await _db.SaveChangesAsync();

                StickyHelper.ClearStickyCache(submission.Subverse);

                return CommandResponse.FromStatus(Status.Success);
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<CommandResponse>(ex);
            }
        }

        public async Task<CommandResponse<Comment>> DistinguishComment(int commentID)
        {
            DemandAuthentication();
            var response = CommandResponse.FromStatus<Comment>(null, Status.Invalid);
            var comment = await this.GetComment(commentID);

            if (comment != null)
            {
                // check to see if request came from comment author
                if (User.Identity.Name == comment.UserName)
                {
                    // check to see if comment author is also sub mod or sub admin for comment sub
                    if (ModeratorPermission.HasPermission(User, comment.Subverse, ModeratorAction.DistinguishContent))
                    {
                        var m = new DapperMulti();

                        var u = new DapperUpdate();
                        //u.Update = $"{SqlFormatter.Table("Comment")} SET \"IsDistinguished\" = {SqlFormatter.ToggleBoolean("\"IsDistinguished\"")}";
                        u.Update = SqlFormatter.UpdateSetBlock($"\"IsDistinguished\" = {SqlFormatter.ToggleBoolean("\"IsDistinguished\"")}", SqlFormatter.Table("Comment"));
                        u.Where = "\"ID\" = @id";
                        u.Parameters.Add("id", commentID);
                        m.Add(u);

                        var s = new DapperQuery();
                        s.Select = $"\"IsDistinguished\" FROM {SqlFormatter.Table("Comment")}";
                        s.Where = "\"ID\" = @id";
                        m.Add(s);

                        //ProTip: The actual execution of code is important.
                        var result = await _db.Connection.ExecuteScalarAsync<bool>(m.ToCommandDefinition());
                        comment.IsDistinguished = result;

                        response = CommandResponse.FromStatus(comment, Status.Success);
                    }
                    else
                    {
                        response.Message = "User does not have permissions to distinquish content";
                        response.Status = Status.Denied;
                    }
                }
                else
                {
                    response.Message = "User can only distinquish owned content";
                    response.Status = Status.Denied;
                }
            }
            else
            {
                response.Message = "Comment can not be found";
                response.Status = Status.Denied;
            }
            return response;
        }
        public Task<IEnumerable<Models.SubverseFlair>> GetSubverseFlair(string subverse)
        {

            var subverseLinkFlairs = _db.SubverseFlair
                .Where(n => n.Subverse == subverse)
                .OrderBy(s => s.Label).ToList();

            return Task.FromResult(subverseLinkFlairs.AsEnumerable());
        }
    }
}
