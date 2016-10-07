using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class CreateSubmissionCommand : Command<CommandResponse<Domain.Models.Submission>>
    {
        private UserSubmission _userSubmission;

        public CreateSubmissionCommand(UserSubmission submission)
        {
            _userSubmission = submission;
        }

        protected override async Task<CommandResponse<Domain.Models.Submission>> ProtectedExecute()
        {
            using (var db = new Repository())
            {
                var result = await  db.PostSubmission(_userSubmission);
                return CommandResponse.Map(result, result.Response.Map());
            }
        }
    }

    public class DeleteSubmissionCommand : CacheCommand<CommandResponse, Data.Models.Submission>
    {
        private int _submissionID = 0;
        private string _reason = null;

        public DeleteSubmissionCommand(int submissionID, string reason = null)
        {
            _submissionID = submissionID;
            _reason = reason;
        }

        protected override async Task<Tuple<CommandResponse, Data.Models.Submission>> CacheExecute()
        {
            var result = await Task.Run(() =>
            {
                using (var db = new Repository())
                {
                    return db.DeleteSubmission(_submissionID, _reason);
                }
            });
            return Tuple.Create(CommandResponse.Successful(), result);
        }

        protected override void UpdateCache(Data.Models.Submission result)
        {
            CacheHandler.Instance.Remove(CachingKey.Submission(result.ID));
            //Legacy item removal
            CacheHandler.Instance.Remove(DataCache.Keys.Submission(result.ID));
        }
    }

    public class EditSubmissionCommand : CacheCommand<CommandResponse<Domain.Models.Submission>, Data.Models.Submission>
    {
        private UserSubmission _submission;
        private int _submissionID;

        public EditSubmissionCommand(int submissionID, UserSubmission submission)
        {
            _submissionID = submissionID;
            _submission = submission;
        }

        protected override async Task<Tuple<CommandResponse<Domain.Models.Submission>, Data.Models.Submission>> CacheExecute()
        {
            var result = await Task.Run(() =>
            {
                using (var db = new Repository())
                {
                    return db.EditSubmission(_submissionID, _submission);
                }
            });
            return Tuple.Create(CommandResponse.Successful(result.Map()), result);
        }

        protected override void UpdateCache(Data.Models.Submission result)
        {
            CacheHandler.Instance.Replace(CachingKey.Submission(result.ID), result);
            //Legacy item removal
            CacheHandler.Instance.Remove(DataCache.Keys.Submission(result.ID));
        }
    }
}
