using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse> RecordUserVote(int voteID, int optionID)
        {
            throw new NotImplementedException();
        }
        public async Task<CommandResponse<Domain.Models.Vote>> SaveVote(Domain.Models.Vote vote)
        {

            DemandAuthentication();

            var newDataModel = VoteDomainMaps.Map(vote);
            var domainModel = vote;

            //UPDATE
            if (newDataModel.ID > 0)
            {
                var existingDataModel = await GetVoteDataModel(newDataModel.ID);
                //Check some basics 
                if (existingDataModel == null)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be found");
                }
                if (!existingDataModel.CreatedBy.IsEqual(User.Identity.Name) && !User.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin }))
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be edited by current user");
                }
                if (existingDataModel.StartDate <= CurrentDate)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be edited once voting has begun");
                }

                //TODO: Verify incoming model ids all belong to this vote
                var restrictionsAreBelongToUs = newDataModel.VoteRestrictions.Where(x => x.ID > 0).All(x => existingDataModel.VoteRestrictions.Any(e => e.ID == x.ID));
                if (!restrictionsAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Restrictions)");
                }
                var optionsAreBelongToUs = newDataModel.VoteOptions.Where(x => x.ID > 0).All(x => existingDataModel.VoteOptions.Any(e => e.ID == x.ID));
                if (!optionsAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Options)");
                }
                var outcomesAreBelongToUs = newDataModel.VoteOptions.Where(x => x.ID > 0).SelectMany(x => x.VoteOutcomes.Where(o => o.ID > 0)).All(x => existingDataModel.VoteOptions.SelectMany(y => y.VoteOutcomes).Any(e => e.ID == x.ID));
                if (!outcomesAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Outcomes)");
                }



                existingDataModel.LastEditDate = CurrentDate;
                existingDataModel.Title = newDataModel.Title;
                existingDataModel.Content = newDataModel.Content;
                existingDataModel.FormattedContent = newDataModel.FormattedContent;
                
                newDataModel.VoteOptions.ForEachIndex((option, index) => {

                    //TODO: Ensure ID belongs to proper vote (aka fuzzy will exploit this)
                    if (option.ID > 0)
                    {
                        //Update Existing
                        var existingOption = existingDataModel.VoteOptions.FirstOrDefault(x => x.ID == option.ID);
                        existingOption.Title = option.Title;
                        existingOption.Content = option.Content;
                        existingOption.FormattedContent = option.FormattedContent;
                        existingOption.SortOrder = index;

                        option.VoteOutcomes.ForEachIndex((outcome, oIndex) => {

                            if (outcome.ID > 0)
                            {
                                var existingOutcome = existingDataModel.VoteOptions[index].VoteOutcomes.FirstOrDefault(x => x.ID == outcome.ID);
                                existingOutcome.Type = outcome.Type;
                                existingOutcome.Data = outcome.Data;
                            }
                            else
                            {
                                var newOutcome = new VoteOutcome();
                                newOutcome.Type = outcome.Type;
                                newOutcome.Data = outcome.Data;
                                existingOption.VoteOutcomes.Add(newOutcome);
                            }
                        });
                        //Remove deleted outcomes
                        var deletedOutcomes = existingOption.VoteOutcomes.Where(c => !option.VoteOutcomes.Any(n => c.ID == n.ID)).ToList();
                        deletedOutcomes.ForEach(x => existingOption.VoteOutcomes.Remove(x));
                    }
                    else
                    {
                        //Add new
                        var newOption = new Data.Models.VoteOption();
                        newOption.Title = option.Title;
                        newOption.Content = option.Content;
                        newOption.FormattedContent = option.FormattedContent;
                        newOption.SortOrder = index;
                        existingDataModel.VoteOptions.Add(newOption);

                        option.VoteOutcomes.ForEachIndex((outcome, oIndex) => {
                            var newOutcome = new VoteOutcome();
                            newOutcome.Type = outcome.Type;
                            newOutcome.Data = outcome.Data;
                            newOption.VoteOutcomes.Add(newOutcome);
                        });

                    }
                });
                //Remove deleted options
                var deletedOptions = existingDataModel.VoteOptions.Where(c => !newDataModel.VoteOptions.Any(n => c.ID == n.ID)).ToList();
                deletedOptions.ForEach(x => existingDataModel.VoteOptions.Remove(x));

                //handle restrictions 
                newDataModel.VoteRestrictions.ForEachIndex((restriction, index) =>
                {
                    if (restriction.ID > 0)
                    {
                        //Update Existing
                        var existingRestriction = existingDataModel.VoteRestrictions.FirstOrDefault(x => x.ID == restriction.ID);
                        existingRestriction.Type = restriction.Type;
                        existingRestriction.Data = restriction.Data;
                    }
                    else
                    {
                        //Add new
                        var newRestriction = new Data.Models.VoteRestriction();
                        newRestriction.Type = restriction.Type;
                        newRestriction.Data = restriction.Data;
                        existingDataModel.VoteRestrictions.Add(newRestriction);
                    }
                });
                //Remove deleted options
                var deletedRestrictions = existingDataModel.VoteRestrictions.Where(c => !newDataModel.VoteRestrictions.Any(n => c.ID == n.ID)).ToList();
                deletedRestrictions.ForEach(x => existingDataModel.VoteRestrictions.Remove(x));

                await _db.SaveChangesAsync();

                return CommandResponse.FromStatus<Domain.Models.Vote>(existingDataModel.Map(), Status.Success);
            }
            //NEW
            else
            {
                newDataModel.CreatedBy = User.Identity.Name;
                newDataModel.CreationDate = CurrentDate;

                //TODO: Set start end dates according to logic
                newDataModel.StartDate = CurrentDate.AddDays(7);
                newDataModel.EndDate = CurrentDate.AddDays(14);

                _db.Vote.Add(newDataModel);
                await _db.SaveChangesAsync();
                domainModel = VoteDomainMaps.Map(newDataModel);

                return CommandResponse.FromStatus<Domain.Models.Vote>(newDataModel.Map(), Status.Success);
            }


        }
        public async Task<Data.Models.Vote> GetVoteDataModel(int id)
        {
            var dataModel = await _db.Vote.Where(x => x.ID == id)
              .Include(x => x.VoteOptions).ThenInclude(o => o.VoteOutcomes)
              .Include(x => x.VoteRestrictions).FirstOrDefaultAsync();
            return dataModel;

        }
        public async Task<Domain.Models.Vote> GetVote(int id)
        {

            var dataModel = await GetVoteDataModel(id);

            var domainModel = VoteDomainMaps.Map(dataModel);

            return domainModel;
        }
    }
}
