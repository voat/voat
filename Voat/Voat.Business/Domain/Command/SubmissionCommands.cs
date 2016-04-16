using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class CreateSubmissionCommand : Command<CommandResponse<Domain.Models.Submission>>
    {
        private UserSubmission _submission;
        private string _subverse;

        public CreateSubmissionCommand(string subverse, UserSubmission submission)
        {
            _subverse = subverse;
            _submission = submission;
        }

        public override async Task<CommandResponse<Domain.Models.Submission>> Execute()
        {
            var result = await Task.Factory.StartNew(() =>
            {
                using (var db = new Repository())
                {
                    return db.PostSubmission(_subverse, _submission);
                }
            });
            return CommandResponse.Map(result, result.Response.Map());
        }
    }

    public class DeleteSubmissionCommand : CacheCommand<CommandResponse, Data.Models.Submission>
    {
        private int _submissionID = 0;

        public DeleteSubmissionCommand(int submissionID)
        {
            _submissionID = submissionID;
        }

        protected override async Task<Tuple<CommandResponse, Data.Models.Submission>> ProtectedExecute()
        {
            var result = await Task.Run(() =>
            {
                using (var db = new Repository())
                {
                    return db.DeleteSubmission(_submissionID);
                }
            });
            return Tuple.Create(CommandResponse.Success(), result);
        }

        protected override void UpdateCache(Data.Models.Submission result)
        {
            throw new NotImplementedException();
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

        protected override async Task<Tuple<CommandResponse<Domain.Models.Submission>, Data.Models.Submission>> ProtectedExecute()
        {
            var result = await Task.Run(() =>
            {
                using (var db = new Repository())
                {
                    return db.EditSubmission(_submissionID, _submission);
                }
            });
            return Tuple.Create(CommandResponse.Success(result.Map()), result);
        }

        protected override void UpdateCache(Data.Models.Submission result)
        {
            CacheHandler.Instance.Replace(CachingKey.Submission(result.ID), result);
        }
    }
}
