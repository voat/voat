using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Models;
using Voat.Voting;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

namespace Voat.Domain
{
    public static class VoteDomainMaps
    {

        public static Vote Map(this CreateVote transform)
        {
            var model = new Vote();
            model.Title = transform.Title;
            model.Content = transform.Content;
            model.Subverse = transform.Subverse;

            foreach (var r in transform.Restrictions)
            {
                var o = r.Construct<VoteRestriction>();
                if (o is ISubverse setSub)
                {
                    setSub.Subverse = model.Subverse;
                }
                model.Restrictions.Add(o);
            }
            transform.Options.ForEach(x =>
            {
                var option = new VoteOption();
                option.Title = x.Title;
                option.Content = x.Content;

                x.Outcomes.ForEach(o =>
                {
                    var outcome = o.Construct<VoteOutcome>();
                    if (outcome is ISubverse setSub)
                    {
                        setSub.Subverse = model.Subverse;
                    }

                    option.Outcomes.Add(outcome);
                });
                model.Options.Add(option);
            });
            return model;
        }
    }
}
