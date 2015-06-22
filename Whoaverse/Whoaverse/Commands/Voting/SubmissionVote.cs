namespace Voat.Commands.Voting
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Models;
    using Queries.Voting;
    using Utils;

    public static class SubmissionVote
    {
        public static async Task<VoteDirection> UpvoteSubmissionAsync(this DbContext context, int submissionId,
            string userName, string clientIpHash)
        {
            var submission = await context.Set<Message>().FindAsync(submissionId).ConfigureAwait(false);

            if (submission.Anonymized)
            {
                return VoteDirection.None;
            }

            var currentVoteStatus =
                await context.Set<Votingtracker>().CheckSubmissionForVoteAsync(userName, submissionId).ConfigureAwait(false);

            switch (currentVoteStatus)
            {
                case VoteStatus.None:
                    if (submission.Name == userName)
                    {
                        return VoteDirection.None;
                    }

                    var votedAlready =
                        await context.Set<Votingtracker>()
                            .AnyAsync(c => c.MessageId == submissionId && c.ClientIpAddress == clientIpHash)
                            .ConfigureAwait(false);

                    if (votedAlready)
                    {
                        return VoteDirection.None;
                    }

                    submission.Likes++;
                    var currentScore = submission.Likes - submission.Dislikes;
                    double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                    double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);
                    submission.Rank = newRank;

                    context.Set<Votingtracker>().Add(new Votingtracker
                    {
                        MessageId = submissionId,
                        UserName = userName,
                        VoteStatus = 1,
                        Timestamp = DateTime.Now,
                        ClientIpAddress = clientIpHash
                    });
                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return VoteDirection.Upvote;

                case VoteStatus.Downvoted:
                    if (submission.Name == userName)
                    {
                        return VoteDirection.None;
                    }
                    submission.Likes++;
                    submission.Dislikes--;

                    currentScore = submission.Likes - submission.Dislikes;
                    submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                    newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);
                    submission.Rank = newRank;

                    // register Turn DownVote To UpVote
                    var votingTracker =
                        await context.Set<Votingtracker>()
                            .FirstOrDefaultAsync(b => b.MessageId == submissionId && b.UserName == userName)
                            .ConfigureAwait(false);

                    if (votingTracker != null)
                    {
                        votingTracker.VoteStatus = 1;
                        votingTracker.Timestamp = DateTime.Now;
                    }
                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return VoteDirection.DownvoteToUpvote;

                case VoteStatus.Upvoted:
                    submission.Likes--;

                    currentScore = submission.Likes - submission.Dislikes;
                    submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                    newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);
                    submission.Rank = newRank;

                    votingTracker = await context.Set<Votingtracker>()
                        .FirstOrDefaultAsync(b => b.MessageId == submissionId && b.UserName == userName)
                        .ConfigureAwait(false);

                    if (votingTracker != null)
                    {
                        context.Set<Votingtracker>().Remove(votingTracker);
                    }

                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return VoteDirection.Downvote;

                default:
                    return VoteDirection.None;
            }
        }
    }
}