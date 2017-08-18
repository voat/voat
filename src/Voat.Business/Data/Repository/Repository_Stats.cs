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

            var result = await _db.Connection.QueryAsync<UserVoteStats>(procedure,
                 new { BeginDate = options.StartDate.Value, EndDate = options.EndDate.Value, RecordCount = options.Count },
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

            var result = await _db.Connection.QueryAsync<UserVoteReceivedStats>(procedure,
                  new { BeginDate = options.StartDate, EndDate = options.EndDate, RecordCount = options.Count },
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

            var result = await _db.Connection.QueryAsync<StatContentItem>(procedure,
               new { BeginDate = options.StartDate, EndDate = options.EndDate, RecordCount = options.Count },
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
                        item.Comment = comments.FirstOrDefault(x => x.ID == item.ID).Map(User);
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
