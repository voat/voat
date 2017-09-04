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

            var inputDataModel = VoteDomainMaps.Map(vote);
            var domainModel = vote;

            //UPDATE
            if (inputDataModel.ID > 0)
            {
                //throw new NotImplementedException("Editing not yet implemented");
                var currentDataModel = await GetVoteDataModel(inputDataModel.ID);
                currentDataModel.LastEditDate = CurrentDate;
                currentDataModel.Title = inputDataModel.Title;
                currentDataModel.Content = inputDataModel.Content;
                currentDataModel.FormattedContent = inputDataModel.FormattedContent;

                inputDataModel.VoteOptions.ForEachIndex((option, index) => {

                    if (option.ID > 0)
                    {
                        //Update Existing
                        var currentOption = currentDataModel.VoteOptions.FirstOrDefault(x => x.ID == option.ID);
                        currentOption.Title = option.Title;
                        currentOption.Content = option.Content;
                        currentOption.FormattedContent = option.FormattedContent;
                        currentOption.SortOrder = index;
                    }
                    else
                    {
                        //Add new
                        var newOption = new Data.Models.VoteOption();
                        newOption.Title = option.Title;
                        newOption.Content = option.Content;
                        newOption.FormattedContent = option.FormattedContent;
                        newOption.SortOrder = index;
                        currentDataModel.VoteOptions.Add(newOption);
                    }
                });
                //Remove deleted options
                var deletedOptions = currentDataModel.VoteOptions.Where(c => !inputDataModel.VoteOptions.Any(n => c.ID == n.ID)).ToList();
                deletedOptions.ForEach(x => currentDataModel.VoteOptions.Remove(x));

                await _db.SaveChangesAsync();

                return CommandResponse.FromStatus<Domain.Models.Vote>(currentDataModel.Map(), Status.Success);
            }
            //NEW
            else
            {
                inputDataModel.CreatedBy = User.Identity.Name;
                inputDataModel.CreationDate = CurrentDate;

                //TODO: Set start end dates according to logic
                inputDataModel.StartDate = CurrentDate.AddDays(7);
                inputDataModel.EndDate = CurrentDate.AddDays(14);

                _db.Vote.Add(inputDataModel);
                await _db.SaveChangesAsync();
                domainModel = VoteDomainMaps.Map(inputDataModel);

                return CommandResponse.FromStatus<Domain.Models.Vote>(inputDataModel.Map(), Status.Success);
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
