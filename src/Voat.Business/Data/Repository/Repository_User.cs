using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        public int UserContributionCount(string userName, ContentType contentType, string subverse = null, DateRange range = null)
        {
            int count = 0;
            if ((contentType & ContentType.Comment) > 0)
            {
                count += UserCommentCount(userName, range, subverse);
            }
            if ((contentType & ContentType.Submission) > 0)
            {
                count += UserSubmissionCount(userName, range, null, subverse);
            }
            return count;
        }

        private int UserCommentCount(string userName, DateRange range = null, string subverse = null, bool? isDeleted = null)
        {
            if (range == null)
            {
                range = new DateRange();
            }
            //var result = (from x in _db.Comment
            //              where
            //                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
            //                && (x.Submission.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) || subverse == null)
            //                && (
            //                    (startDate.HasValue && x.CreationDate >= startDate.Value)
            //                    &&
            //                    (endDate.HasValue && x.CreationDate <= endDate.Value)
            //                    )
            //              select x).Count();
            var q = new DapperQuery();
            q.Select = $"COUNT(*) FROM {SqlFormatter.Table("Comment", "c", null, "NOLOCK")}";
            q.Where = "c.\"UserName\" = @UserName";
            q.Parameters.Add("UserName", userName);

            if (range.StartDate.HasValue)
            {
                q.Append(x => x.Where, "c.\"CreationDate\" >= @StartDate");
                //Bug in Dapper this line looses TimeZome info see: https://github.com/npgsql/npgsql/issues/972#issuecomment-218745473
                //q.Parameters.Add("StartDate", range.StartDate.Value);
                q.Parameters.AddDynamicParams(new { StartDate = range.StartDate.Value });
            }

            if (range.EndDate.HasValue)
            {
                q.Append(x => x.Where, "c.\"CreationDate\" <= @EndDate");
                //Bug in Dapper this line looses TimeZome info see: https://github.com/npgsql/npgsql/issues/972#issuecomment-218745473
                //q.Parameters.Add("EndDate", range.EndDate.Value);
                q.Parameters.AddDynamicParams(new { EndDate = range.EndDate.Value });
            }

            if (isDeleted.HasValue)
            {
                q.Append(x => x.Where, "\"IsDeleted\" = @IsDeleted");
                q.Parameters.Add("IsDeleted", isDeleted.Value);
            }

            if (!String.IsNullOrEmpty(subverse))
            {
                q.Append(x => x.Select, $" INNER JOIN {SqlFormatter.Table("Submission", "s", null, "NOLOCK")} ON c.\"SubmissionID\" = s.\"ID\"");
                q.Append(x => x.Where, "s.\"Subverse\" = @Subverse");
                q.Parameters.Add("Subverse", subverse);
            }

            var count = _db.Connection.ExecuteScalar<int>(q.ToString(), q.Parameters);

            return count;
        }

        private int UserSubmissionCount(string userName, DateRange range, SubmissionType? type = null, string subverse = null, bool? isDeleted = null)
        {
            if (range == null)
            {
                range = new DateRange();
            }
            var q = new DapperQuery();
            q.Select = $"COUNT(*) FROM {SqlFormatter.Table("Submission", null, null, "NOLOCK")}";
            q.Where = "\"UserName\" = @UserName";
            q.Parameters.Add("UserName", userName);

            if (range.StartDate.HasValue)
            {
                q.Append(x => x.Where, "\"CreationDate\" >= @StartDate");
                //Bug in Dapper this line looses TimeZome info see: https://github.com/npgsql/npgsql/issues/972#issuecomment-218745473
                //q.Parameters.Add("StartDate", range.StartDate.Value);
                q.Parameters.AddDynamicParams(new { StartDate = range.StartDate.Value });
            }

            if (range.EndDate.HasValue)
            {
                q.Append(x => x.Where, "\"CreationDate\" <= @EndDate");
                //Bug in Dapper this line looses TimeZome info see: https://github.com/npgsql/npgsql/issues/972#issuecomment-218745473
                //q.Parameters.Add("EndDate", range.EndDate.Value);
                q.Parameters.AddDynamicParams(new { EndDate = range.EndDate.Value });
            }

            if (isDeleted.HasValue)
            {
                q.Append(x => x.Where, "\"IsDeleted\" = @IsDeleted");
                q.Parameters.Add("IsDeleted", isDeleted.Value);
            }


            if (type.HasValue)
            {
                q.Append(x => x.Where, "\"Type\" = @Type");
                q.Parameters.Add("Type", (int)type.Value);
            }

            if (!String.IsNullOrEmpty(subverse))
            {
                q.Append(x => x.Where, "\"Subverse\" = @Subverse");
                q.Parameters.Add("Subverse", subverse);
            }

            var count = _db.Connection.ExecuteScalar<int>(q.ToString(), q.Parameters);

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

        public Score UserContributionPoints(string userName, ContentType contentType, string subverse = null, bool isReceived = true, TimeSpan? timeSpan = null, DateTime? cutOffDate = null)
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

    }
}
