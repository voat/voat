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
        public static Domain.Models.Vote Map(this Data.Models.Vote entity)
        {
            var vote = new Domain.Models.Vote();

            vote.ID = entity.ID;
            vote.Title = entity.Title;
            vote.Content = entity.Content;
            vote.FormattedContent = entity.FormattedContent;
            vote.Subverse = entity.Subverse;
            vote.SubmissionID = entity.SubmissionID;
            vote.StartDate = entity.StartDate;
            vote.ShowCurrentStats = entity.DisplayStatistics;
            vote.EndDate = entity.EndDate;
            vote.CreationDate = entity.CreationDate;
            vote.CreatedBy = entity.CreatedBy;

            entity.VoteOptions?.ForEach(x => {
                var newOption = new Domain.Models.VoteOption();

                newOption.ID = x.ID;
                newOption.Title = x.Title;
                newOption.Content = x.Content;
                newOption.FormattedContent = x.FormattedContent;
                newOption.SortOrder = x.SortOrder;

                x.VoteOutcomes?.ForEach(o => {
                    var obj = VoteItem.Deserialize<VoteOutcome>(o.Data);
                    newOption.Outcomes.Add(obj);
                });

                vote.Options.Add(newOption);
            });

            entity.VoteRestrictions?.ForEach(x => {
                var obj = VoteItem.Deserialize<VoteRestriction>(x.Data);
                vote.Restrictions.Add(obj);
            });

            return vote;
        }
        public static Data.Models.Vote Map(this Domain.Models.Vote entity)
        {
            var vote = new Data.Models.Vote();
            vote.ID = entity.ID;
            vote.Title = entity.Title;
            vote.Content = entity.Content;
            vote.FormattedContent = entity.FormattedContent;
            vote.DisplayStatistics = entity.ShowCurrentStats;


            vote.VoteOptions = new List<Data.Models.VoteOption>();
            entity.Options.ForEach(x => {
                var newOption = new Data.Models.VoteOption();

                newOption.Title = x.Title;
                newOption.Content = x.Content;
                newOption.FormattedContent = x.FormattedContent;
                newOption.SortOrder = entity.Options.IndexOf(x);

                newOption.VoteOutcomes = new List<Data.Models.VoteOutcome>();
                x.Outcomes.ForEach(o => {
                    var newOutcome = new Data.Models.VoteOutcome();
                    newOutcome.Type = o.TypeName;
                    newOutcome.Data = o.Serialize();
                    newOption.VoteOutcomes.Add(newOutcome);
                });
                vote.VoteOptions.Add(newOption);
            });

            vote.VoteRestrictions = new List<Data.Models.VoteRestriction>();
            entity.Restrictions.ForEach(x => {
                var newRestriction = new Data.Models.VoteRestriction();

                newRestriction.Type = x.TypeName;
                newRestriction.Data = x.Serialize();

                vote.VoteRestrictions.Add(newRestriction);
            });

            return vote;
        }

        public static Vote Map(this CreateVote transform)
        {
            var model = new Vote();
            model.ID = transform.ID;
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
