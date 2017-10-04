using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Query;
using Voat.Voting.Models;

namespace Voat.Domain.Models
{
    public class VoteAggregate
    {
        public static async Task<VoteAggregate> Load(int voteID)
        {
            
            var v = new QueryVote(voteID);
            var vote = await v.ExecuteAsync();

            return await Load(vote);

        }
        public static async Task<VoteAggregate> Load(Vote vote)
        {
            var aggregate = new VoteAggregate();

            aggregate.Vote = vote;

            if (aggregate.Vote != null)
            {

                var s = new QueryVoteStatistics(aggregate.Vote.ID);
                aggregate.Statistics = await s.ExecuteAsync();

                if (aggregate.Statistics.Raw.Count == 0)
                {

                    //For testing, vote has no stats so lets generate some
                    var generate = new Func<Dictionary<int, int>>(() => {

                        var dict = new Dictionary<int, int>();
                        var random = new Random();

                        foreach (var o in aggregate.Vote.Options)
                        {
                            dict.Add(o.ID, random.Next(1, 5000));
                        }

                        return dict;
                    });

                    aggregate.Statistics.Raw.Add(VoteRestrictionStatus.Certified, generate());
                    aggregate.Statistics.Raw.Add(VoteRestrictionStatus.Uncertified, generate());
                    aggregate.Statistics.Friendly = null;

                }



                if (aggregate.Vote.SubmissionID > 0)
                {
                    var sub = new QuerySubmission(aggregate.Vote.SubmissionID);
                    aggregate.Submission = await sub.ExecuteAsync();
                }
            }
            return aggregate;
        }

        public Domain.Models.Vote Vote { get; set; }
        public VoteStatistics Statistics { get; set; }
        public Submission Submission { get; set; }
    }
}
