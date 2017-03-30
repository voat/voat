using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {

        public async Task<Statistics<IEnumerable<UserVoteStats>>> UserVotesGivenStatistics(SearchOptions options)
        {
            var procedure = SqlFormatter.Object("usp_Reports_UserVoteGivenStats");

            var result = await _db.Database.Connection.QueryAsync<UserVoteStats>(procedure,
                 new { BeginDate = options.StartDate.Value, endDate = options.EndDate.Value, RecordCount = options.Count },
                commandType: System.Data.CommandType.StoredProcedure);

            return new Statistics<IEnumerable<UserVoteStats>>() {
                BeginDate = options.StartDate.Value,
                EndDate = options.EndDate.Value,
                Data = result
            };

        }
        public async Task<Statistics<IEnumerable<UserVoteReceivedStats>>> UserVotesReceivedStatistics(SearchOptions options)
        {
            var procedure = SqlFormatter.Object("usp_Reports_UserVoteReceivedStats");

            var result = await _db.Database.Connection.QueryAsync<UserVoteReceivedStats>(procedure,
                  new { BeginDate = options.StartDate, endDate = options.EndDate, RecordCount = options.Count },
                commandType: System.Data.CommandType.StoredProcedure);

            return new Statistics<IEnumerable<UserVoteReceivedStats>>()
            {
                BeginDate = options.StartDate.Value,
                EndDate = options.EndDate.Value,
                Data = result
            };
        }

        public async Task<Statistics<IEnumerable<StatContentItem>>> HighestVotedContentStatistics(SearchOptions options)
        {
            //Get content items
            var procedure = SqlFormatter.Object("usp_Reports_HighestVotedContent");

            var result = await _db.Database.Connection.QueryAsync<StatContentItem>(procedure,
               new { BeginDate = options.StartDate, endDate = options.EndDate, RecordCount = options.Count },
               commandType: System.Data.CommandType.StoredProcedure);

            //Load content items
            var comments = await GetComments(result.Where(x => x.ContentType == ContentType.Comment).Select(x => x.ID).ToArray());
            var submissions = await GetSubmissions(result.Where(x => x.ContentType == ContentType.Submission).Select(x => x.ID).ToArray());

            foreach (var item in result)
            {
                switch (item.ContentType)
                {
                    case ContentType.Submission:
                        item.Submission = submissions.FirstOrDefault(x => x.ID == item.ID).Map();
                        break;
                    case ContentType.Comment:
                        item.Comment = comments.FirstOrDefault(x => x.ID == item.ID).Map();
                        break;

                }
            }
            return new Statistics<IEnumerable<StatContentItem>>()
            {
                BeginDate = options.StartDate.Value,
                EndDate = options.EndDate.Value,
                Data = result
            };
        }
    }
}
