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
using System.Threading.Tasks;
using Voat.Domain.Models;
using Dapper;
using Voat.Utilities;
using Voat.Data.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<Tuple<int, IEnumerable<Domain.Models.SubverseBan>>> GetModLogBannedUsers(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                var data = (from b in db.SubverseBan
                            where b.Subverse.ToLower() == subverse.ToLower()
                            select new Domain.Models.SubverseBan
                            {
                                CreatedBy = b.CreatedBy,
                                CreationDate = b.CreationDate,
                                Reason = b.Reason,
                                Subverse = b.Subverse,
                                ID = b.ID,
                                UserName = b.UserName
                            });
                //This is nasty imo
                var count = data.Count();
                data = data.OrderByDescending(x => x.CreationDate).Skip(options.Index).Take(options.Count);
                var results = await data.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return Tuple.Create(count, results.AsEnumerable());
            }
        }
        public async Task<IEnumerable<Data.Models.SubmissionRemovalLog>> GetModLogRemovedSubmissions(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                var data = (from b in db.SubmissionRemovalLog
                            join s in db.Submission on b.SubmissionID equals s.ID
                            where s.Subverse.ToLower() == subverse.ToLower()
                            select new SubmissionRemovalLog()
                            {
                                SubmissionID = b.SubmissionID,
                                Submission = s,
                                CreationDate = b.CreationDate,
                                Moderator = b.Moderator,
                                Reason = b.Reason
                            });

                var data2 = data.OrderByDescending(x => x.CreationDate).Skip(options.Index).Take(options.Count);
                var results = await data2.ToListAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return results;
            }
        }
        public async Task<IEnumerable<Domain.Models.CommentRemovalLog>> GetModLogRemovedComments(string subverse, SearchOptions options)
        {
            using (var db = new VoatDataContext(CONSTANTS.CONNECTION_READONLY))
            {
                var data = (from b in db.CommentRemovalLog
                            join c in db.Comment on b.CommentID equals c.ID
                            join s in db.Submission on c.SubmissionID equals s.ID
                            where s.Subverse.ToLower() == subverse.ToLower()
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
    }
}
